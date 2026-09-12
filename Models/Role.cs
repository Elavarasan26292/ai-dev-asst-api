using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ai_dev_asst_api.Models;

[Table("ROLES")]
public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ROLEID")]
    public int RoleId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("ROLENAME")]
    public string RoleName { get; set; } = string.Empty;

    [Column("CREATEDAT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("CREATEDBYUSERID")]
    public int? CreatedByUserId { get; set; }

    [Column("LASTUPDATEDAT")]
    public DateTime? LastUpdatedAt { get; set; }

    [Column("ISDELETED")]
    public bool IsDeleted { get; set; } = false;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
