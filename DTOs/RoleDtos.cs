using System.ComponentModel.DataAnnotations;

namespace ai_dev_asst_api.DTOs;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters.")]
    public string RoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one permission ID is required.")]
    [MinLength(1, ErrorMessage = "At least one permission ID is required.")]
    public List<Guid> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters.")]
    public string RoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one permission ID is required.")]
    [MinLength(1, ErrorMessage = "At least one permission ID is required.")]
    public List<Guid> PermissionIds { get; set; } = new();
}

public class RoleResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionResponse> Permissions { get; set; } = new();
}

public class PermissionResponse
{
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
}
