using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ai_dev_asst_api.Models;

[Table("PERMISSIONS")]
public class Permission
{
    [Key]
    [Column("PERMISSIONID")]
    public Guid PermissionId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("PERMISSIONNAME")]
    public string PermissionName { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
