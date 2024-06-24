using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Middleware;
public class TokenService
{
    private readonly IUserRepository userRepository;
    private readonly SigningAudienceCertificate signingAudienceCertificate;

    public TokenService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
        signingAudienceCertificate = new SigningAudienceCertificate();
    }

    public string GetToken(string email)
    {
        User user = userRepository.GetUser(email);
        SecurityTokenDescriptor tokenDescriptor = GetTokenDescriptor(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        string token = tokenHandler.WriteToken(securityToken);

        return token;
    }

    private SecurityTokenDescriptor GetTokenDescriptor(User user)
    {
        const int expiringDays = 7;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(user.Claims()),
            Expires = DateTime.UtcNow.AddDays(expiringDays),
            SigningCredentials = signingAudienceCertificate.GetAudienceSigningKey()
        };

        return tokenDescriptor;
    }
}