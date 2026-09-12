using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ai_dev_asst_api.Models;

[Table("REFRESHTOKENS")]
public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID")]
    public int Id { get; set; }

    [Column("USERID")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("TOKEN")]
    public string Token { get; set; } = string.Empty;

    [Column("EXPIRESAT")]
    public DateTime ExpiresAt { get; set; }

    [Column("ISREVOKED")]
    public bool IsRevoked { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
