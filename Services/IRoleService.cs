using ai_dev_asst_api.DTOs;

namespace ai_dev_asst_api.Services;

public interface IRoleService
{
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, int? createdByUserId = null);
    Task<RoleResponse> UpdateRoleAsync(int roleId, UpdateRoleRequest request);
    Task DeleteRoleAsync(int roleId);
    Task<List<RoleResponse>> GetAllRolesAsync();
    Task<List<PermissionResponse>> GetAllPermissionsAsync();
}
