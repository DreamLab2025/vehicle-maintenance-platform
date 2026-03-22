using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Application.Services.Implements
{
    public class UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger) : IUserService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<UserService> _logger = logger;

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
    }
}
