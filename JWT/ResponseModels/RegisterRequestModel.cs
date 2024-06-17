using System.ComponentModel.DataAnnotations;

namespace JWT.ResponseModels;

public class RegisterRequestModel
{
    [Required]
    public string UserName { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}