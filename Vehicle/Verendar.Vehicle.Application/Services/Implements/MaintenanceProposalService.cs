using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class MaintenanceProposalService(
        ILogger<MaintenanceProposalService> logger,
        IUnitOfWork unitOfWork,
        IMaintenanceRecordService maintenanceRecordService) : IMaintenanceProposalService
    {
        private readonly ILogger<MaintenanceProposalService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMaintenanceRecordService _maintenanceRecordService = maintenanceRecordService;

        public async Task<ApiResponse<List<MaintenanceProposalResponse>>> GetPendingByVehicleAsync(
            Guid userId, Guid vehicleId, PaginationRequest pagination, CancellationToken ct = default)
        {
            if (!await VehicleBelongsToUserAsync(vehicleId, userId, ct))
                return ApiResponse<List<MaintenanceProposalResponse>>.ForbiddenResponse(
                    "Bạn không có quyền xem xe này.");

            pagination.Normalize();
            var (proposals, total) = await _unitOfWork.MaintenanceProposals
                .GetPagedPendingByVehicleIdAsync(vehicleId, pagination.PageNumber, pagination.PageSize, ct);

            var categoryNames = await ResolveCategoryNamesAsync(proposals.SelectMany(p => p.Items), ct);

            var items = proposals.Select(p => p.ToResponse(categoryNames)).ToList();

            return ApiResponse<List<MaintenanceProposalResponse>>.SuccessPagedResponse(
                items,
                total,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách đề xuất bảo dưỡng thành công.");
        }

        public async Task<ApiResponse<MaintenanceProposalResponse>> UpdateAsync(
            Guid userId, Guid vehicleId, Guid proposalId, UpdateProposalRequest request, CancellationToken ct = default)
        {
            if (!await VehicleBelongsToUserAsync(vehicleId, userId, ct))
                return ApiResponse<MaintenanceProposalResponse>.ForbiddenResponse(
                    "Bạn không có quyền chỉnh sửa xe này.");

            var proposal = await _unitOfWork.MaintenanceProposals.GetByIdTrackedWithItemsAsync(proposalId, ct);
            if (proposal is null || proposal.UserVehicleId != vehicleId)
                return ApiResponse<MaintenanceProposalResponse>.NotFoundResponse("Không tìm thấy đề xuất bảo dưỡng.");

            if (proposal.Status != ProposalStatus.Pending)
                return ApiResponse<MaintenanceProposalResponse>.FailureResponse(
                    "Đề xuất này đã được áp dụng, không thể chỉnh sửa.");

            if (request.OdometerAtService.HasValue)
                proposal.OdometerAtService = request.OdometerAtService;

            if (request.Notes is not null)
                proposal.Notes = request.Notes;

            if (request.Items is { Count: > 0 })
            {
                var itemMap = proposal.Items.ToDictionary(i => i.Id);
                foreach (var itemReq in request.Items)
                {
                    if (!itemMap.TryGetValue(itemReq.Id, out var item))
                        return ApiResponse<MaintenanceProposalResponse>.FailureResponse(
                            $"Item '{itemReq.Id}' không thuộc đề xuất này.");
                    item.UpdatesTracking = itemReq.UpdatesTracking;
                }
            }

            proposal.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.MaintenanceProposals.UpdateAsync(proposalId, proposal);
            await _unitOfWork.SaveChangesAsync(ct);

            var categoryNames = await ResolveCategoryNamesAsync(proposal.Items, ct);
            return ApiResponse<MaintenanceProposalResponse>.SuccessResponse(
                proposal.ToResponse(categoryNames),
                "Cập nhật đề xuất bảo dưỡng thành công.");
        }

        public async Task<ApiResponse<ApplyProposalResult>> ApplyAsync(
            Guid userId, Guid vehicleId, Guid proposalId, CancellationToken ct = default)
        {
            if (!await VehicleBelongsToUserAsync(vehicleId, userId, ct))
                return ApiResponse<ApplyProposalResult>.ForbiddenResponse("Bạn không có quyền thực hiện hành động này.");

            var proposal = await _unitOfWork.MaintenanceProposals.GetByIdTrackedWithItemsAsync(proposalId, ct);
            if (proposal is null || proposal.UserVehicleId != vehicleId)
                return ApiResponse<ApplyProposalResult>.NotFoundResponse("Không tìm thấy đề xuất bảo dưỡng.");

            if (proposal.Status != ProposalStatus.Pending)
                return ApiResponse<ApplyProposalResult>.FailureResponse(
                    "Đề xuất này đã được áp dụng.");

            if (!proposal.OdometerAtService.HasValue || proposal.OdometerAtService <= 0)
                return ApiResponse<ApplyProposalResult>.FailureResponse(
                    "Vui lòng cập nhật số km hiện tại trước khi xác nhận bảo dưỡng.");

            // Only items with a PartCategory can be recorded in MaintenanceRecord
            var trackableItems = proposal.Items
                .Where(i => i.PartCategoryId.HasValue)
                .ToList();

            if (trackableItems.Count == 0)
            {
                _logger.LogWarning(
                    "Proposal {ProposalId} has no trackable items — applying with empty record",
                    proposalId);
            }

            // Batch-load PartCategories to get their slugs
            var categoryIds = trackableItems
                .Select(i => i.PartCategoryId!.Value)
                .Distinct()
                .ToList();

            var categories = categoryIds.Count > 0
                ? await _unitOfWork.PartCategories.GetAllAsync(pc => categoryIds.Contains(pc.Id))
                : [];

            var categoryMap = categories.ToDictionary(pc => pc.Id);

            // Build RecordItemInput list — only items with a resolved category
            var recordItems = new List<RecordItemInput>();
            var trackingUpdatedNames = new List<string>();

            foreach (var item in trackableItems)
            {
                if (!categoryMap.TryGetValue(item.PartCategoryId!.Value, out var category))
                {
                    _logger.LogWarning(
                        "PartCategory {PartCategoryId} not found — skipping item '{ItemName}' in proposal {ProposalId}",
                        item.PartCategoryId, item.ItemName, proposalId);
                    continue;
                }

                recordItems.Add(new RecordItemInput
                {
                    PartCategorySlug = category.Slug,
                    GarageProductId = item.GarageProductId,
                    CustomPartName = item.ItemName,
                    CustomKmInterval = item.RecommendedKmInterval,
                    CustomMonthsInterval = item.RecommendedMonthsInterval,
                    Price = item.Price,
                    UpdatesTracking = item.UpdatesTracking
                });

                if (item.UpdatesTracking)
                    trackingUpdatedNames.Add(category.Name);
            }

            var createRequest = new CreateRecordRequest
            {
                UserVehicleId = vehicleId,
                ServiceDate = proposal.ServiceDate,
                OdometerAtService = proposal.OdometerAtService!.Value,
                GarageName = proposal.BranchName,
                TotalCost = proposal.TotalAmount,
                Notes = proposal.Notes,
                Items = recordItems
            };

            var createResult = await _maintenanceRecordService.CreateMaintenanceRecordAsync(userId, vehicleId, createRequest);
            if (!createResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to create MaintenanceRecord from proposal {ProposalId}: {Message}",
                    proposalId, createResult.Message);
                return ApiResponse<ApplyProposalResult>.FailureResponse(createResult.Message);
            }

            // Mark proposal as applied
            proposal.Status = ProposalStatus.Applied;
            proposal.AppliedAt = DateTime.UtcNow;
            proposal.ResultMaintenanceRecordId = createResult.Data?.MaintenanceRecordId;
            proposal.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.MaintenanceProposals.UpdateAsync(proposalId, proposal);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Proposal {ProposalId} applied → MaintenanceRecord {RecordId}",
                proposalId, createResult.Data?.MaintenanceRecordId);

            return ApiResponse<ApplyProposalResult>.SuccessResponse(
                new ApplyProposalResult
                {
                    MaintenanceRecordId = createResult.Data!.MaintenanceRecordId,
                    ServiceDate = proposal.ServiceDate,
                    OdometerAtService = proposal.OdometerAtService!.Value,
                    TrackingUpdated = trackingUpdatedNames
                },
                "Đã xác nhận bảo dưỡng và cập nhật lịch sử thành công.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private async Task<bool> VehicleBelongsToUserAsync(Guid vehicleId, Guid userId, CancellationToken ct)
        {
            var vehicle = await _unitOfWork.UserVehicles.FindOneAsync(
                v => v.Id == vehicleId && v.UserId == userId && v.DeletedAt == null);
            return vehicle is not null;
        }

        private async Task<IReadOnlyDictionary<Guid, string>> ResolveCategoryNamesAsync(
            IEnumerable<MaintenanceProposalItem> items, CancellationToken ct)
        {
            var ids = items
                .Where(i => i.PartCategoryId.HasValue)
                .Select(i => i.PartCategoryId!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0) return new Dictionary<Guid, string>();

            var categories = await _unitOfWork.PartCategories.GetAllAsync(pc => ids.Contains(pc.Id));
            return categories.ToDictionary(pc => pc.Id, pc => pc.Name);
        }
    }
}
