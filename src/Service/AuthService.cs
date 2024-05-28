using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ecommerce.EntityFramework.Table;
using ecommerce.Models;
using Microsoft.IdentityModel.Tokens;

namespace ecommerce.service;

public class AuthService
{

    public AuthService()
    {
       Console.WriteLine($"JWT Issuer: {Environment.GetEnvironmentVariable("Jwt__Issuer")}");
    }

    public string GenerateJwt(User user)
    {
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? throw new InvalidOperationException("JWT Issuer is missing in environment variables.");
var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? throw new InvalidOperationException("JWT Audience is missing in environment variables.");
var key = Encoding.ASCII.GetBytes(jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    //Role-based
                    new Claim(ClaimTypes.Role, user.Role == Role.Admin ? "Admin" : "User"),
                    //Claim-based
                    new Claim("IsBanned", user.IsBanned ? "true" : "false"),
                }
            ),

            Expires = DateTime.UtcNow.AddMinutes(45),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),

            Issuer = jwtIssuer,
            Audience = jwtAudience,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
