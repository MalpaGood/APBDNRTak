using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT.Models;

[Table("WebUsers")]
public class WebUser
{
    [Key]
    [Column("ID_User")]
    public int UserId { get; set; }
    
    [Column("Login")]
    [MaxLength(20)]
    public string Login { get; set; }

    [Column("Password")]
    public string Password { get; set; }
}