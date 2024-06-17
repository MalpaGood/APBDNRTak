namespace JWT.ResponseModels;

public class LoginResponseModel
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}