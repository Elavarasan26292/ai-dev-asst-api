using ai_dev_asst_api.DTOs;

namespace ai_dev_asst_api.Services;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, int? createdByUserId = null);
    Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task DeleteUserAsync(int userId);
    Task<List<UserResponse>> GetAllUsersAsync();
}
