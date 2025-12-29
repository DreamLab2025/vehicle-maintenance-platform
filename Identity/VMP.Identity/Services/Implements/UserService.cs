using VMP.Common.Shared;
using VMP.Identity.Dtos;
using VMP.Identity.Mappings;
using VMP.Identity.Repositories.Interfaces;
using VMP.Identity.Services.Interfaces;

namespace VMP.Identity.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        public UserService(ILogger<UserService> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(PaginationRequest request)
        {
            var result = await _userRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                null,
                q => q.OrderByDescending(u => u.CreatedAt)
            );

            var userDtos = result.Items.Select(u => u.ToDto()).ToList();

            return ApiResponse<List<UserDto>>.SuccessPagedResponse(
                userDtos,
                result.TotalCount,
                request.PageNumber,
                request.PageSize
            );
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
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
