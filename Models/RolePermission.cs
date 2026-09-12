using System.ComponentModel.DataAnnotations.Schema;

namespace ai_dev_asst_api.Models;

[Table("ROLEPERMISSIONS")]
public class RolePermission
{
    [Column("ROLEID")]
    public int RoleId { get; set; }

    [Column("PERMISSIONID")]
    public Guid PermissionId { get; set; }

    [ForeignKey("RoleId")]
    public Role Role { get; set; } = null!;

    [ForeignKey("PermissionId")]
    public Permission Permission { get; set; } = null!;
}
