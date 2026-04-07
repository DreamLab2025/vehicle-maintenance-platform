using System.Security.Cryptography;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;

namespace Verendar.Identity.Application.Services.Implements
{
    public class UserService(
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<UserService> logger) : IUserService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
        private readonly ILogger<UserService> _logger = logger;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(UserFilterRequest request)
        {
            request.Normalize();

            var name = request.Name?.Trim().ToLower();
            var phone = request.Phone?.Trim();
            var roles = request.Roles?.Where(r => Enum.IsDefined(typeof(UserRole), r)).ToList();
            var descending = request.IsDescending != false;

            var users = await _unitOfWork.Users.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                u => (string.IsNullOrEmpty(name) || u.FullName.ToLower().Contains(name))
                  && (string.IsNullOrEmpty(phone) || (u.PhoneNumber != null && u.PhoneNumber.Contains(phone)))
                  && (roles == null || roles.Count == 0 || u.Roles.Any(r => roles.Contains(r))),
                BuildOrderBy(request.SortBy, descending)
            );

            var userDtos = users.Items.Select(u => u.ToDto()).ToList();

            return ApiResponse<List<UserDto>>.SuccessPagedResponse(
                userDtos,
                users.TotalCount,
                request.PageNumber,
                request.PageSize,
                "Lấy danh sách người dùng thành công."
            );
        }

        private static Func<IQueryable<User>, IOrderedQueryable<User>> BuildOrderBy(string? sortBy, bool descending) =>
            sortBy?.ToLower() switch
            {
                "name" => descending
                    ? q => q.OrderByDescending(u => u.FullName)
                    : q => q.OrderBy(u => u.FullName),
                "email" => descending
                    ? q => q.OrderByDescending(u => u.Email)
                    : q => q.OrderBy(u => u.Email),
                "phone" => descending
                    ? q => q.OrderByDescending(u => u.PhoneNumber)
                    : q => q.OrderBy(u => u.PhoneNumber),
                _ => descending
                    ? q => q.OrderByDescending(u => u.CreatedAt)
                    : q => q.OrderBy(u => u.CreatedAt),
            };

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("GetUserById: user not found {UserId}", userId);
                return ApiResponse<UserDto>.NotFoundResponse("Người dùng không tồn tại.");
            }

            return ApiResponse<UserDto>.SuccessResponse(user.ToDto(), "Lấy thông tin người dùng thành công.");
        }

        public async Task<ApiResponse<CreateMechanicResponse>> CreateMechanicAsync(CreateMechanicRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);

            var existing = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (existing != null)
            {
                _logger.LogWarning("CreateMechanic: email already registered {Email}", email);
                return ApiResponse<CreateMechanicResponse>.ConflictResponse("Email đã được đăng ký.");
            }

            var actualPassword = !string.IsNullOrWhiteSpace(request.Password)
                ? request.Password
                : "Mechanic@" + RandomNumberGenerator.GetInt32(100000, 999999);

            var user = new User
            {
                Id = Guid.CreateVersion7(),
                FullName = request.FullName,
                Email = email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = string.Empty,
                EmailVerified = false,
                PhoneNumberVerified = false,
                Roles = [UserRole.Mechanic],
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, actualPassword);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created mechanic user {UserId} for email {Email}", user.Id, email);

            try
            {
                await _publishEndpoint.Publish(new MemberAccountCreatedEvent
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = email,
                    TempPassword = actualPassword,
                    Role = nameof(UserRole.Mechanic)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish MemberAccountCreatedEvent for mechanic {UserId}", user.Id);
            }

            return ApiResponse<CreateMechanicResponse>.CreatedResponse(
                new CreateMechanicResponse(user.Id, actualPassword),
                "Tạo tài khoản mechanic thành công."
            );
        }

        public async Task<ApiResponse<CreateManagerResponse>> CreateManagerAsync(CreateManagerRequest request)
        {
            var email = EmailHelper.Normalize(request.Email);

            var existing = await _unitOfWork.Users.FindOneAsync(u => u.Email == email);
            if (existing != null)
            {
                _logger.LogWarning("CreateManager: email already registered {Email}", email);
                return ApiResponse<CreateManagerResponse>.ConflictResponse("Email đã được đăng ký.");
            }

            var actualPassword = !string.IsNullOrWhiteSpace(request.Password)
                ? request.Password
                : "Manager@" + RandomNumberGenerator.GetInt32(100000, 999999);

            var user = new User
            {
                Id = Guid.CreateVersion7(),
                FullName = request.FullName,
                Email = email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = string.Empty,
                EmailVerified = false,
                PhoneNumberVerified = false,
                Roles = [UserRole.GarageManager],
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = null
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, actualPassword);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created manager user {UserId} for email {Email}", user.Id, email);

            try
            {
                await _publishEndpoint.Publish(new MemberAccountCreatedEvent
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = email,
                    TempPassword = actualPassword,
                    Role = nameof(UserRole.GarageManager)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish MemberAccountCreatedEvent for manager {UserId}", user.Id);
            }

            return ApiResponse<CreateManagerResponse>.CreatedResponse(
                new CreateManagerResponse(user.Id, actualPassword),
                "Tạo tài khoản manager thành công."
            );
        }

        public async Task<ApiResponse<bool>> AssignRoleAsync(Guid userId, UserRole role)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AssignRole: user not found {UserId}", userId);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            if (user.Roles.Contains(role))
            {
                _logger.LogInformation("AssignRole: user {UserId} already has role {Role}", userId, role);
                return ApiResponse<bool>.SuccessResponse(true, "Role đã tồn tại.");
            }

            // Replace list to ensure EF Core change tracking picks up the modification
            user.Roles = [.. user.Roles, role];
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("AssignRole: user {UserId} assigned role {Role}", userId, role);

            try
            {
                await _publishEndpoint.Publish(new ForceTokenRefreshEvent
                {
                    UserId = userId,
                    Reason = $"Role {role} assigned"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish ForceTokenRefreshEvent for user {UserId}", userId);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Gán role thành công.");
        }

        public async Task<ApiResponse<bool>> RevokeRoleAsync(Guid userId, UserRole role)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("RevokeRole: user not found {UserId}", userId);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            if (!user.Roles.Contains(role))
            {
                _logger.LogInformation("RevokeRole: user {UserId} does not have role {Role}", userId, role);
                return ApiResponse<bool>.SuccessResponse(true, "User không có role này.");
            }

            // Replace list to ensure EF Core change tracking picks up the modification
            user.Roles = user.Roles.Where(r => r != role).ToList();
            // Invalidate refresh token to force re-login on next refresh
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("RevokeRole: user {UserId} revoked role {Role}", userId, role);

            return ApiResponse<bool>.SuccessResponse(true, "Thu hồi role thành công.");
        }

        public async Task<ApiResponse<bool>> BulkDeactivateAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.ToList();
            if (ids.Count == 0)
                return ApiResponse<bool>.SuccessResponse(true, "Không có tài khoản nào cần vô hiệu hóa.");

            var users = await _unitOfWork.Users.GetAllAsync(u => ids.Contains(u.Id) && u.DeletedAt == null);

            foreach (var user in users)
            {
                user.RefreshToken = string.Empty;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("BulkDeactivate: deactivated {Count} user accounts", users.Count());



            return ApiResponse<bool>.SuccessResponse(true, $"Đã vô hiệu hóa {users.Count()} tài khoản.");
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default)
        {
            var normalizedEmail = EmailHelper.Normalize(request.Email);
            var existing = await _unitOfWork.Users.FindOneAsync(u => u.Email == normalizedEmail);
            if (existing != null)
            {
                _logger.LogWarning("CreateUser: email already registered {Email}", normalizedEmail);
                return ApiResponse<UserDto>.ConflictResponse("Email đã được đăng ký.");
            }

            var user = request.ToNewUser(string.Empty);
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("CreateUser: created user {UserId} for email {Email}", user.Id, normalizedEmail);

            return ApiResponse<UserDto>.CreatedResponse(user.ToDto(), "Tạo người dùng thành công.");
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("UpdateUser: user not found {UserId}", userId);
                return ApiResponse<UserDto>.NotFoundResponse("Người dùng không tồn tại.");
            }

            var normalizedEmail = EmailHelper.Normalize(request.Email);
            var duplicate = await _unitOfWork.Users.FindOneAsync(u => u.Email == normalizedEmail && u.Id != userId);
            if (duplicate != null)
            {
                _logger.LogWarning("UpdateUser: email already in use {Email}", normalizedEmail);
                return ApiResponse<UserDto>.ConflictResponse("Email đã được sử dụng bởi tài khoản khác.");
            }

            var oldEmail = user.Email;
            var oldRoles = user.Roles.OrderBy(r => r).ToList();
            var passwordChanged = !string.IsNullOrWhiteSpace(request.Password);

            user.FullName = request.FullName.Trim();
            user.Email = normalizedEmail;
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.EmailVerified = request.EmailVerified;
            user.PhoneNumberVerified = request.PhoneNumberVerified;
            user.Roles = request.Roles.Distinct().ToList();

            if (passwordChanged)
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password!.Trim());

            var newRolesOrdered = user.Roles.OrderBy(r => r).ToList();
            var shouldInvalidateSessions = passwordChanged
                || oldEmail != normalizedEmail
                || !oldRoles.SequenceEqual(newRolesOrdered);

            if (shouldInvalidateSessions)
            {
                user.RefreshToken = string.Empty;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                try
                {
                    await _publishEndpoint.Publish(new ForceTokenRefreshEvent
                    {
                        UserId = userId,
                        Reason = "User profile updated"
                    }, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to publish ForceTokenRefreshEvent for user {UserId}", userId);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("UpdateUser: updated user {UserId}", userId);

            return ApiResponse<UserDto>.SuccessResponse(user.ToDto(), "Cập nhật người dùng thành công.");
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(Guid userId, Guid actingUserId, CancellationToken ct = default)
        {
            if (userId == actingUserId)
            {
                _logger.LogWarning("DeleteUser: attempted self-delete {UserId}", userId);
                return ApiResponse<bool>.FailureResponse("Không thể xóa tài khoản của chính bạn.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("DeleteUser: user not found {UserId}", userId);
                return ApiResponse<bool>.NotFoundResponse("Người dùng không tồn tại.");
            }

            user.DeletedAt = DateTime.UtcNow;
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("DeleteUser: soft-deleted user {UserId}", userId);

            try
            {
                await _publishEndpoint.Publish(new ForceTokenRefreshEvent
                {
                    UserId = userId,
                    Reason = "User deleted"
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish ForceTokenRefreshEvent for deleted user {UserId}", userId);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Xóa người dùng thành công.");
        }
    }
}
