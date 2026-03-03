using Microsoft.Extensions.Logging;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Entities;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Infrastructure.Services
{
    public class UserService(ILogger<UserService> logger, IUnitOfWork unitOfWork) : IUserService
    {
        private readonly ILogger<UserService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest paginationRequest)
        {
            paginationRequest.Normalize();
            var users = await _unitOfWork.Users.GetPagedAsync(
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                null,
                q => q.OrderByDescending(u => u.CreatedAt)
            );

            if (users.Items == null)
            {
                return ApiResponse<List<UserDto>>.FailureResponse("Không tìm thấy danh sách người dùng.");
            }

            var userItems = users.Items ?? new List<User>();
            var userDtos = userItems.Select(u => u.ToDto()).ToList();

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
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.FailureResponse("Người dùng không tồn tại.");
                }
                return ApiResponse<UserDto>.SuccessResponse(user.ToDto(), "Lấy thông tin người dùng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user by ID: {UserId}", userId);
                return ApiResponse<UserDto>.FailureResponse("Đã xảy ra lỗi trong quá trình tìm kiếm.");
            }
        }
    }
}
