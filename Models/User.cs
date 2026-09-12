using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ai_dev_asst_api.Models;

[Table("USERS")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("USERID")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("FIRSTNAME")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("LASTNAME")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("PHONE")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("EMAIL")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("PASSWORDHASH")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("ROLEID")]
    public int RoleId { get; set; }

    [Column("CREATEDAT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("CREATEDBYUSERID")]
    public int? CreatedByUserId { get; set; }

    [Column("LASTUPDATEDAT")]
    public DateTime? LastUpdatedAt { get; set; }

    [MaxLength(50)]
    [Column("THEME")]
    public string Theme { get; set; } = "default";

    [Column("ISDELETED")]
    public bool IsDeleted { get; set; } = false;

    [ForeignKey("RoleId")]
    public Role Role { get; set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
