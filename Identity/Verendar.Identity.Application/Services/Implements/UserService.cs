using System.Security.Cryptography;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Application.Shared.Helpers;
using Verendar.Identity.Domain.Repositories.Interfaces;
using Verender.Identity.Contracts.Events;
using Verendar.Identity.Domain.Entities;

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

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest)
        {
            paginationRequest.Normalize();
            var users = await _unitOfWork.Users.GetPagedAsync(
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                null,
                q => q.OrderByDescending(u => u.CreatedAt)
            );

            var userDtos = users.Items.Select(u => u.ToDto()).ToList();

            return ApiResponse<List<UserDto>>.SuccessPagedResponse(
                userDtos,
                users.TotalCount,
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                "Lấy danh sách người dùng thành công."
            );
        }

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

            var tempPassword = "Mechanic@" + RandomNumberGenerator.GetInt32(100000, 999999);

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

            user.PasswordHash = _passwordHasher.HashPassword(user, tempPassword);

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
                    TempPassword = tempPassword,
                    Role = nameof(UserRole.Mechanic)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish MemberAccountCreatedEvent for mechanic {UserId}", user.Id);
            }

            return ApiResponse<CreateMechanicResponse>.CreatedResponse(
                new CreateMechanicResponse(user.Id),
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

            var tempPassword = "Manager@" + RandomNumberGenerator.GetInt32(100000, 999999);

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

            user.PasswordHash = _passwordHasher.HashPassword(user, tempPassword);

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
                    TempPassword = tempPassword,
                    Role = nameof(UserRole.GarageManager)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish MemberAccountCreatedEvent for manager {UserId}", user.Id);
            }

            return ApiResponse<CreateManagerResponse>.CreatedResponse(
                new CreateManagerResponse(user.Id),
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
            user.Roles = [..user.Roles, role];
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
    }
}
