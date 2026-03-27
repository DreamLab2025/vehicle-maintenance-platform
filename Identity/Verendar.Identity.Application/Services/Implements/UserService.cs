using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Application.Shared.Helpers;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Application.Services.Implements
{
    public class UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger) : IUserService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
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

            return ApiResponse<CreateManagerResponse>.CreatedResponse(
                new CreateManagerResponse(user.Id),
                "Tạo tài khoản manager thành công."
            );
        }
    }
}
