using System.ComponentModel.DataAnnotations;

namespace JWT.RequestModels;

public class RegisterUserRequestModel
{
    [MaxLength(50)] public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;
}