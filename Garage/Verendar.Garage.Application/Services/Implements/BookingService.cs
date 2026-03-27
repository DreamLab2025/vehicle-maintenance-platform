using MassTransit;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Contracts.Events;
using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Application.Services.Implements;

public class BookingService(
    ILogger<BookingService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IGarageIdentityContactClient identityContactClient,
    IVehicleGarageClient vehicleGarageClient) : IBookingService
{
    private readonly ILogger<BookingService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IGarageIdentityContactClient _identityContactClient = identityContactClient;
    private readonly IVehicleGarageClient _vehicleGarageClient = vehicleGarageClient;

    public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(
        Guid userId,
        CreateBookingRequest request,
        CancellationToken ct = default)
    {
        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == request.GarageBranchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy chi nhánh.");

        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy garage.");

        if (garage.Status != GarageStatus.Active)
            return ApiResponse<BookingResponse>.FailureResponse("Garage chưa hoạt động, không thể đặt lịch.");

        if (branch.Status != BranchStatus.Active)
            return ApiResponse<BookingResponse>.FailureResponse("Chi nhánh không hoạt động, không thể đặt lịch.");

        if (request.Items.Count == 0)
            return ApiResponse<BookingResponse>.FailureResponse("Booking phải có ít nhất một mục.", 422);

        var scheduledUtc = NormalizeToUtc(request.ScheduledAt);
        if (scheduledUtc <= DateTime.UtcNow)
            return ApiResponse<BookingResponse>.FailureResponse("Thời gian đặt lịch phải ở tương lai.");

        // Resolve all line items and compute total
        var resolvedItems = await ResolveCreateLineItemsAsync(request.Items, branch.Id, ct);
        if (resolvedItems.Error is not null)
            return ApiResponse<BookingResponse>.FailureResponse(resolvedItems.Error, 422);

        decimal totalAmount = resolvedItems.LineItems!.Sum(i => i.BookedItemPrice.Amount);

        var booking = new Booking
        {
            GarageBranchId = branch.Id,
            UserId = userId,
            UserVehicleId = request.UserVehicleId,
            ScheduledAt = scheduledUtc,
            Status = BookingStatus.Pending,
            Note = request.Note,
            BookedTotalPrice = new Money { Amount = totalAmount, Currency = "VND" },
            PaymentId = null
        };

        foreach (var lineItem in resolvedItems.LineItems!)
            booking.LineItems.Add(lineItem);

        AddHistory(booking, BookingStatus.Pending, BookingStatus.Pending, userId);

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("CreateBooking: {BookingId} user={UserId} branch={BranchId} items={ItemCount}",
            booking.Id, userId, branch.Id, booking.LineItems.Count);

        try
        {
            var firstItemName = resolvedItems.LineItems.Count > 0
                ? resolvedItems.LineItems[0].Product?.Name ?? resolvedItems.LineItems[0].Service?.Name ?? resolvedItems.LineItems[0].Bundle?.Name ?? string.Empty
                : string.Empty;

            await _publishEndpoint.Publish(new BookingCreatedEvent
            {
                BookingId = booking.Id,
                UserId = userId,
                UserVehicleId = booking.UserVehicleId,
                GarageBranchId = branch.Id,
                BranchName = branch.Name,
                ItemsSummary = resolvedItems.LineItems.Count == 1
                    ? firstItemName
                    : $"{firstItemName} và {resolvedItems.LineItems.Count - 1} mục khác",
                TotalAmount = totalAmount,
                ScheduledAt = booking.ScheduledAt
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish BookingCreatedEvent for booking {BookingId}", booking.Id);
        }

        var created = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(booking.Id, ct);
        if (created is null)
            return ApiResponse<BookingResponse>.FailureResponse("Không thể tải booking vừa tạo.");

        return ApiResponse<BookingResponse>.CreatedResponse(created.ToResponse(), "Đặt lịch thành công.");
    }

    public async Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(
        Guid bookingId,
        Guid viewerId,
        CancellationToken ct = default)
    {
        var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy booking.");

        if (!await CanViewerAccessBookingAsync(booking, viewerId, ct))
            return ApiResponse<BookingResponse>.ForbiddenResponse("Bạn không có quyền xem booking này.");

        var isAssignedMechanic = await IsAssignedMechanicViewerAsync(booking, viewerId, ct);
        BookingCustomerSummary? customer = null;
        BookingVehicleSummary? vehicle = null;
        if (isAssignedMechanic)
        {
            customer = await _identityContactClient.GetCustomerContactAsync(booking.UserId, ct);
            vehicle = await _vehicleGarageClient.GetUserVehicleForBookingAsync(booking.UserId, booking.UserVehicleId, ct);
        }

        return ApiResponse<BookingResponse>.SuccessResponse(
            booking.ToResponse(customer, vehicle),
            "Lấy thông tin booking thành công.");
    }

    public async Task<ApiResponse<List<BookingListItemResponse>>> GetBookingsAsync(
        Guid currentUserId,
        bool assignedToMe,
        Guid? branchId,
        Guid? userId,
        PaginationRequest pagination,
        CancellationToken ct = default)
    {
        if (assignedToMe && (branchId.HasValue || userId.HasValue))
            return ApiResponse<List<BookingListItemResponse>>.FailureResponse(
                "Không kết hợp assignedToMe với branchId hoặc userId.");

        if (branchId.HasValue && userId.HasValue)
            return ApiResponse<List<BookingListItemResponse>>.FailureResponse(
                "Chỉ chọn một trong branchId hoặc userId.");

        if (assignedToMe)
            return await GetBookingsAssignedToMeAsync(currentUserId, pagination, ct);

        if (branchId.HasValue)
            return await GetBookingsByBranchIdAsync(branchId.Value, currentUserId, pagination, ct);

        if (userId.HasValue)
            return await GetBookingsByUserIdAsync(userId.Value, currentUserId, pagination, ct);

        return ApiResponse<List<BookingListItemResponse>>.FailureResponse(
            "Thiếu tham số: userId, branchId hoặc assignedToMe=true.");
    }

    private async Task<ApiResponse<List<BookingListItemResponse>>> GetBookingsByUserIdAsync(
        Guid requestedUserId,
        Guid currentUserId,
        PaginationRequest request,
        CancellationToken ct = default)
    {
        request.Normalize();

        if (requestedUserId != currentUserId)
            return ApiResponse<List<BookingListItemResponse>>.ForbiddenResponse(
                "Chỉ được xem danh sách booking của chính mình.");

        var (items, totalCount) = await _unitOfWork.Bookings.GetPagedByUserIdAsync(
            requestedUserId,
            request.PageNumber,
            request.PageSize,
            ct);

        return ApiResponse<List<BookingListItemResponse>>.SuccessPagedResponse(
            items.Select(b => b.ToListItemResponse()).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            "Lấy danh sách booking thành công.");
    }

    private async Task<ApiResponse<List<BookingListItemResponse>>> GetBookingsByBranchIdAsync(
        Guid branchId,
        Guid viewerId,
        PaginationRequest request,
        CancellationToken ct = default)
    {
        request.Normalize();

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<List<BookingListItemResponse>>.NotFoundResponse("Không tìm thấy chi nhánh.");

        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<List<BookingListItemResponse>>.NotFoundResponse("Không tìm thấy garage.");

        var isOwner = garage.OwnerId == viewerId;
        if (!isOwner)
        {
            var manager = await _unitOfWork.Members.FindOneAsync(m =>
                m.UserId == viewerId
                && m.GarageBranchId == branchId
                && m.Role == MemberRole.Manager
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null);
            if (manager is null)
                return ApiResponse<List<BookingListItemResponse>>.ForbiddenResponse(
                    "Bạn không có quyền xem danh sách booking của chi nhánh này.");
        }

        var (items, totalCount) = await _unitOfWork.Bookings.GetPagedByBranchIdAsync(
            branchId,
            request.PageNumber,
            request.PageSize,
            ct);

        return ApiResponse<List<BookingListItemResponse>>.SuccessPagedResponse(
            items.Select(b => b.ToListItemResponse()).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            "Lấy danh sách booking theo chi nhánh thành công.");
    }

    private async Task<ApiResponse<List<BookingListItemResponse>>> GetBookingsAssignedToMeAsync(
        Guid mechanicUserId,
        PaginationRequest request,
        CancellationToken ct = default)
    {
        request.Normalize();

        var memberIds = await GetActiveMechanicMemberIdsAsync(mechanicUserId, ct);
        if (memberIds.Count == 0)
            return ApiResponse<List<BookingListItemResponse>>.ForbiddenResponse(
                "Tài khoản không phải thợ máy hoặc chưa được kích hoạt.");

        var (items, totalCount) = await _unitOfWork.Bookings.GetPagedByMechanicMemberIdsAsync(
            memberIds,
            request.PageNumber,
            request.PageSize,
            ct);

        return ApiResponse<List<BookingListItemResponse>>.SuccessPagedResponse(
            items.Select(b => b.ToListItemResponse()).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            "Lấy danh sách booking được giao thành công.");
    }

    public async Task<ApiResponse<BookingResponse>> AssignMechanicAsync(
        Guid bookingId,
        Guid actorId,
        AssignBookingRequest request,
        CancellationToken ct = default)
    {
        var booking = await _unitOfWork.Bookings.GetByIdTrackedWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy booking.");

        if (booking.Status is not (BookingStatus.Pending or BookingStatus.AwaitingConfirmation))
            return ApiResponse<BookingResponse>.FailureResponse("Chỉ có thể gán thợ khi booking đang chờ xử lý.");

        var garage = booking.GarageBranch.Garage;
        if (!await CanOwnerOrManagerBranchAsync(garage.OwnerId, booking.GarageBranchId, actorId))
            return ApiResponse<BookingResponse>.ForbiddenResponse("Bạn không có quyền gán thợ cho booking này.");

        var mechanic = await _unitOfWork.Members.FindOneAsync(m =>
            m.Id == request.GarageMemberId
            && m.GarageBranchId == booking.GarageBranchId
            && m.Role == MemberRole.Mechanic
            && m.Status == MemberStatus.Active
            && m.DeletedAt == null);

        if (mechanic is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy thợ máy thuộc chi nhánh này.");

        var from = booking.Status;
        booking.MechanicId = mechanic.Id;
        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        AddHistory(booking, from, BookingStatus.Confirmed, actorId);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _publishEndpoint.Publish(new BookingConfirmedEvent
            {
                BookingId = booking.Id,
                CustomerUserId = booking.UserId,
                GarageBranchId = booking.GarageBranchId,
                MechanicMemberId = mechanic.Id,
                MechanicDisplayName = mechanic.DisplayName
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish BookingConfirmedEvent for booking {BookingId}", booking.Id);
        }

        var reloaded = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId, ct);
        return ApiResponse<BookingResponse>.SuccessResponse(
            reloaded!.ToResponse(),
            "Đã gán thợ và xác nhận booking.");
    }

    public async Task<ApiResponse<BookingResponse>> UpdateMechanicStatusAsync(
        Guid bookingId,
        Guid mechanicUserId,
        UpdateBookingMechanicStatusRequest request,
        CancellationToken ct = default)
    {
        var booking = await _unitOfWork.Bookings.GetByIdTrackedWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return ApiResponse<BookingResponse>.NotFoundResponse("Không tìm thấy booking.");

        if (booking.MechanicId is null)
            return ApiResponse<BookingResponse>.FailureResponse("Booking chưa được gán thợ.");

        var mechanicMember = await _unitOfWork.Members.FindOneAsync(m =>
            m.Id == booking.MechanicId && m.DeletedAt == null);
        if (mechanicMember is null || mechanicMember.UserId != mechanicUserId)
            return ApiResponse<BookingResponse>.ForbiddenResponse("Bạn không phải thợ được gán cho booking này.");

        var from = booking.Status;

        if (request.Status == BookingStatus.InProgress)
        {
            if (from != BookingStatus.Confirmed)
                return ApiResponse<BookingResponse>.FailureResponse("Chỉ có thể bắt đầu khi booking đã xác nhận.");
        }
        else if (request.Status == BookingStatus.Completed)
        {
            if (from != BookingStatus.InProgress)
                return ApiResponse<BookingResponse>.FailureResponse("Chỉ có thể hoàn tất khi đang thực hiện.");
        }
        else
            return ApiResponse<BookingResponse>.FailureResponse("Trạng thái cập nhật không hợp lệ.");

        booking.Status = request.Status;
        booking.UpdatedAt = DateTime.UtcNow;
        if (request.Status == BookingStatus.Completed)
        {
            booking.CompletedAt = DateTime.UtcNow;
            if (request.CurrentOdometer.HasValue)
                booking.CurrentOdometer = request.CurrentOdometer;
        }

        AddHistory(booking, from, request.Status, mechanicUserId);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            if (request.Status == BookingStatus.InProgress)
            {
                await _publishEndpoint.Publish(new BookingStatusChangedEvent
                {
                    BookingId = booking.Id,
                    CustomerUserId = booking.UserId,
                    GarageBranchId = booking.GarageBranchId,
                    FromStatus = from.ToString(),
                    ToStatus = request.Status.ToString(),
                    ChangedAt = DateTime.UtcNow
                }, ct);
            }
            else if (request.Status == BookingStatus.Completed)
            {
                var lineItems = await ResolveCompletedLineItemsAsync(booking, ct);
                await _publishEndpoint.Publish(new BookingCompletedEvent
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    UserVehicleId = booking.UserVehicleId,
                    GarageBranchId = booking.GarageBranchId,
                    BranchName = booking.GarageBranch.Name,
                    CurrentOdometer = booking.CurrentOdometer,
                    CompletedAt = booking.CompletedAt ?? DateTime.UtcNow,
                    TotalAmount = booking.BookedTotalPrice.Amount,
                    LineItems = lineItems
                }, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish status event for booking {BookingId} → {Status}", booking.Id, request.Status);
        }

        var reloaded = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId, ct);
        return ApiResponse<BookingResponse>.SuccessResponse(
            reloaded!.ToResponse(),
            "Cập nhật tiến độ booking thành công.");
    }

    public async Task<ApiResponse<bool>> CancelBookingAsync(
        Guid bookingId,
        Guid actorId,
        string? reason,
        CancellationToken ct = default)
    {
        var booking = await _unitOfWork.Bookings.GetByIdTrackedWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return ApiResponse<bool>.NotFoundResponse("Không tìm thấy booking.");

        if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
            return ApiResponse<bool>.FailureResponse("Không thể hủy booking ở trạng thái này.");

        if (!await CanCancelBookingAsync(booking, actorId))
            return ApiResponse<bool>.ForbiddenResponse("Bạn không có quyền hủy booking này.");

        var from = booking.Status;
        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;

        AddHistory(booking, from, BookingStatus.Cancelled, actorId, reason);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _publishEndpoint.Publish(new BookingCancelledEvent
            {
                BookingId = booking.Id,
                CustomerUserId = booking.UserId,
                GarageBranchId = booking.GarageBranchId,
                Reason = reason,
                CancelledAt = DateTime.UtcNow
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish BookingCancelledEvent for booking {BookingId}", booking.Id);
        }

        return ApiResponse<bool>.SuccessResponse(true, "Đã hủy booking.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private record ResolvedLineItems(List<BookingLineItem>? LineItems, string? Error);

    /// <summary>
    /// Resolve CreateBookingLineItemRequest list → BookingLineItem entities with prices.
    /// Validates items belong to the branch and are Active.
    /// </summary>
    private async Task<ResolvedLineItems> ResolveCreateLineItemsAsync(
        List<CreateBookingLineItemRequest> requests,
        Guid branchId,
        CancellationToken ct)
    {
        var result = new List<BookingLineItem>();

        for (var i = 0; i < requests.Count; i++)
        {
            var req = requests[i];
            var setCount = (req.ProductId.HasValue ? 1 : 0) + (req.ServiceId.HasValue ? 1 : 0) + (req.BundleId.HasValue ? 1 : 0);
            if (setCount != 1)
                return new ResolvedLineItems(null, $"Mục #{i + 1}: phải chỉ định đúng một trong ProductId, ServiceId hoặc BundleId.");

            decimal itemPrice;

            if (req.ProductId.HasValue)
            {
                var product = await _unitOfWork.GarageProducts.GetByIdWithInstallationAsync(req.ProductId.Value, ct);
                if (product is null || product.GarageBranchId != branchId || product.DeletedAt != null)
                    return new ResolvedLineItems(null, $"Mục #{i + 1}: sản phẩm không tồn tại hoặc không thuộc chi nhánh này.");
                if (product.Status != ProductStatus.Active)
                    return new ResolvedLineItems(null, $"Mục #{i + 1}: sản phẩm '{product.Name}' không khả dụng.");

                itemPrice = product.MaterialPrice.Amount;
                if (req.IncludeInstallation)
                {
                    if (product.InstallationService is null)
                        return new ResolvedLineItems(null, $"Mục #{i + 1}: sản phẩm '{product.Name}' không có dịch vụ lắp đặt.");
                    itemPrice += product.InstallationService.LaborPrice.Amount;
                }
            }
            else if (req.ServiceId.HasValue)
            {
                var service = await _unitOfWork.GarageServices.FindOneAsync(
                    s => s.Id == req.ServiceId.Value && s.GarageBranchId == branchId && s.DeletedAt == null);
                if (service is null)
                    return new ResolvedLineItems(null, $"Mục #{i + 1}: dịch vụ không tồn tại hoặc không thuộc chi nhánh này.");
                if (service.Status != ProductStatus.Active)
                    return new ResolvedLineItems(null, $"Mục #{i + 1}: dịch vụ '{service.Name}' không khả dụng.");

                itemPrice = service.LaborPrice.Amount;
            }
            else // BundleId
            {
                var bundle = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(req.BundleId!.Value, ct);
                if (bundle is null || bundle.GarageBranchId != branchId || bundle.DeletedAt != null)
                    return new ResolvedLineItems(null, $"Mục #{i + 1}: combo không tồn tại hoặc không thuộc chi nhánh này.");
                if (bundle.Status != ProductStatus.Active)
                    return new ResolvedLineItems(null, $"Mục #{i + 1}: combo '{bundle.Name}' không khả dụng.");

                var subTotal = GarageBundleMappings.CalculateSubTotal(bundle);
                itemPrice = GarageBundleMappings.CalculateFinalPrice(bundle, subTotal);
            }

            result.Add(new BookingLineItem
            {
                ProductId = req.ProductId,
                ServiceId = req.ServiceId,
                BundleId = req.BundleId,
                IncludeInstallation = req.IncludeInstallation,
                BookedItemPrice = new Money { Amount = itemPrice, Currency = "VND" },
                SortOrder = req.SortOrder > 0 ? req.SortOrder : i
            });
        }

        return new ResolvedLineItems(result, null);
    }

    /// <summary>
    /// Flatten booking line items at completion time for the BookingCompletedEvent.
    /// Bundles are expanded into individual items so Vehicle service can update PartTracking.
    /// </summary>
    private async Task<List<BookingCompletedLineItem>> ResolveCompletedLineItemsAsync(
        Booking booking,
        CancellationToken ct)
    {
        var result = new List<BookingCompletedLineItem>();

        foreach (var lineItem in booking.LineItems.OrderBy(i => i.SortOrder))
        {
            if (lineItem.ProductId.HasValue)
            {
                var product = await _unitOfWork.GarageProducts.FindOneAsync(
                    p => p.Id == lineItem.ProductId.Value);
                if (product is not null)
                {
                    result.Add(new BookingCompletedLineItem
                    {
                        GarageProductId = product.Id,
                        PartCategoryId = product.PartCategoryId,
                        ItemName = product.Name,
                        UpdatesTracking = product.PartCategoryId.HasValue,
                        Price = lineItem.BookedItemPrice.Amount,
                        RecommendedKmInterval = product.ManufacturerKmInterval,
                        RecommendedMonthsInterval = product.ManufacturerMonthInterval
                    });
                }
            }
            else if (lineItem.ServiceId.HasValue)
            {
                var service = await _unitOfWork.GarageServices.FindOneAsync(
                    s => s.Id == lineItem.ServiceId.Value);
                if (service is not null)
                {
                    result.Add(new BookingCompletedLineItem
                    {
                        GarageServiceId = service.Id,
                        ItemName = service.Name,
                        UpdatesTracking = false,
                        Price = lineItem.BookedItemPrice.Amount
                    });
                }
            }
            else if (lineItem.BundleId.HasValue)
            {
                // Expand bundle items
                var bundle = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(lineItem.BundleId.Value, ct);
                if (bundle is not null)
                {
                    foreach (var bundleItem in bundle.Items.OrderBy(bi => bi.SortOrder))
                    {
                        if (bundleItem.ProductId.HasValue && bundleItem.Product is not null)
                        {
                            result.Add(new BookingCompletedLineItem
                            {
                                GarageProductId = bundleItem.Product.Id,
                                PartCategoryId = bundleItem.Product.PartCategoryId,
                                ItemName = bundleItem.Product.Name,
                                UpdatesTracking = bundleItem.Product.PartCategoryId.HasValue,
                                Price = 0, // price is on the bundle line item, not per sub-item
                                RecommendedKmInterval = bundleItem.Product.ManufacturerKmInterval,
                                RecommendedMonthsInterval = bundleItem.Product.ManufacturerMonthInterval
                            });
                        }
                        else if (bundleItem.ServiceId.HasValue && bundleItem.Service is not null)
                        {
                            result.Add(new BookingCompletedLineItem
                            {
                                GarageServiceId = bundleItem.Service.Id,
                                ItemName = bundleItem.Service.Name,
                                UpdatesTracking = false,
                                Price = 0
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Bundle {BundleId} not found at completion of booking {BookingId}",
                        lineItem.BundleId, booking.Id);
                }
            }
        }

        return result;
    }

    private static void AddHistory(
        Booking booking,
        BookingStatus from,
        BookingStatus to,
        Guid changedBy,
        string? note = null)
    {
        booking.StatusHistory.Add(new BookingStatusHistory
        {
            FromStatus = from,
            ToStatus = to,
            ChangedByUserId = changedBy,
            ChangedAt = DateTime.UtcNow,
            Note = note
        });
    }

    private async Task<bool> CanCancelBookingAsync(Booking booking, Guid actorId)
    {
        if (booking.UserId == actorId)
            return true;
        var garage = booking.GarageBranch.Garage;
        return await CanOwnerOrManagerBranchAsync(garage.OwnerId, booking.GarageBranchId, actorId);
    }

    private async Task<bool> CanOwnerOrManagerBranchAsync(Guid garageOwnerId, Guid branchId, Guid actorId)
    {
        if (garageOwnerId == actorId)
            return true;

        var manager = await _unitOfWork.Members.FindOneAsync(m =>
            m.UserId == actorId
            && m.GarageBranchId == branchId
            && m.Role == MemberRole.Manager
            && m.Status == MemberStatus.Active
            && m.DeletedAt == null);

        return manager is not null;
    }

    private async Task<List<Guid>> GetActiveMechanicMemberIdsAsync(Guid userId, CancellationToken ct)
    {
        var members = await _unitOfWork.Members.GetAllAsync(m =>
            m.UserId == userId
            && m.Role == MemberRole.Mechanic
            && m.Status == MemberStatus.Active
            && m.DeletedAt == null);
        return members.Select(m => m.Id).ToList();
    }

    private async Task<bool> IsAssignedMechanicViewerAsync(Booking booking, Guid viewerId, CancellationToken ct)
    {
        if (booking.MechanicId is null)
            return false;
        var member = await _unitOfWork.Members.FindOneAsync(m => m.Id == booking.MechanicId && m.DeletedAt == null);
        return member is not null && member.UserId == viewerId;
    }

    private async Task<bool> CanViewerAccessBookingAsync(Booking booking, Guid viewerId, CancellationToken ct)
    {
        if (booking.UserId == viewerId)
            return true;

        var garage = booking.GarageBranch.Garage;
        if (garage.OwnerId == viewerId)
            return true;

        var manager = await _unitOfWork.Members.FindOneAsync(
            m => m.UserId == viewerId
                && m.GarageBranchId == booking.GarageBranchId
                && m.Role == MemberRole.Manager
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null);

        if (manager is not null)
            return true;

        return await IsAssignedMechanicViewerAsync(booking, viewerId, ct);
    }

    private static DateTime NormalizeToUtc(DateTime dt) =>
        dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
}
