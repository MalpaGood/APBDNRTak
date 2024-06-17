using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT.Contexts;
using JWT.Exeptions;
using JWT.Models;
using JWT.RequestModels;
using JWT.ResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using JWT.RequestModels;

namespace JWT.Services;


public interface IWebUserService
{
    Task<LoginResponseModel> LoginUserAsync(LoginRequestModel loginRequest, IConfiguration configuration);
    Task<RefreshTokenResponseModel> RefreshUserToken(RefreshTokenRequestModel request, IConfiguration configuration);
    Task RegisterUserAsync(RegisterUserRequestModel requestModel);
}

public class WebWebUserService(DatabaseContext context) : IWebUserService
{
    public async Task<LoginResponseModel> LoginUserAsync(LoginRequestModel loginRequest, IConfiguration configuration)
    {
        var user = await context.WebUsers.Where(u => u.Login == loginRequest.Login).FirstOrDefaultAsync();
    
        if (user is not null)
        {
            var hashedPassword = new PasswordHasher<WebUser>();
            if (hashedPassword.VerifyHashedPassword(user, user.Password, loginRequest.Password) ==
                PasswordVerificationResult.Failed)
            {
                throw new CustomException("Login or password is invalid");
            }
            
            var token = CreateToken(configuration, loginRequest.Login);
            var refreshToken = CreateRefreshToken(configuration, loginRequest.Login);
            
            return new LoginResponseModel()
            {
                Token = token,
                RefreshToken = refreshToken
            };
        }
        else
        {
            throw new CustomException("Login or password is invalid");
        }
    }
    
    //rejestracja uytkownika
    public async Task RegisterUserAsync(RegisterUserRequestModel requestModel)
    {
        var isAvible = !(await context.WebUsers.AnyAsync(u => u.Login == requestModel.Login));
        
        if (isAvible)
        {
            if (requestModel.Login.Contains('@'))
            {
                if (!requestModel.Login.Contains('.'))
                {
                    var newWebUser = new WebUser()
                    {
                        Login = requestModel.Login
                    };
        
                    var hashedPassword = new PasswordHasher<WebUser>();
                    var password = hashedPassword.HashPassword(newWebUser, requestModel.Password);

                    newWebUser.Password = password;

                    await context.WebUsers.AddAsync(newWebUser);
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new CustomException("Login provided isnt an email");
                }
            }
            else
            {
                throw new CustomException("Login provided isnt an email");
            }
        }
        else
        {
            throw new CustomException("User already exists");
        }
    }

    public async Task<RefreshTokenResponseModel> RefreshUserToken(RefreshTokenRequestModel request,
        IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            tokenHandler.ValidateToken(request.RefreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:RefIssuer"],
                ValidAudience = configuration["JWT:RefAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:RefKey"]!))
            }, out SecurityToken validatedToken);

            
            var oldToken = tokenHandler.ReadJwtToken(request.RefreshToken);
            
            var token =
                CreateToken(configuration, oldToken.Claims.First(c => c.Type == "login").Value);
            
            var refreshToken = CreateRefreshToken(configuration,oldToken.Claims.First(c =>c.Type == "login").Value);
            
            return new RefreshTokenResponseModel()
            {
                Token = token,
                RefreshToken = refreshToken
            };

        }
        catch
        {
            throw new CustomException("Token is invalid");
        }
    }

    
    //Tworzenie tokenu
    private string CreateToken(IConfiguration configuration, string login)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescription = new SecurityTokenDescriptor
        {
            Issuer = configuration["JWT:Issuer"],
            Audience = configuration["JWT:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(15),
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new Claim("login", login)
            })
        };
        
        var token = tokenHandler.CreateToken(tokenDescription);
        return tokenHandler.WriteToken(token);
    }
    
    //Tworzenie tokenu odswiezenia
    private string CreateRefreshToken(IConfiguration configuration, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var refTokenDescription = new SecurityTokenDescriptor
        {
            Issuer = configuration["JWT:RefIssuer"],
            Audience = configuration["JWT:RefAudience"],
            Expires = DateTime.UtcNow.AddDays(2),
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new Claim("username", username)
            })
        };

        var token = tokenHandler.CreateToken(refTokenDescription);
        return tokenHandler.WriteToken(token);
    }
}