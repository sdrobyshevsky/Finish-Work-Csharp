using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Middleware;

public class JwtBuilder : IJwtBuilder
{

    private readonly JwtOptions _options;
    private readonly IUserRepository _userRepository;

    public JwtBuilder(IOptions<JwtOptions> options, IUserRepository userRepository)
    {
        _options = options.Value;
        _userRepository = userRepository;
    }

    public string GetToken(Guid userId)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
        var user = _userRepository.FindUser(userId);
        var role = user.IsAdmin ? "Admin" : "User";
        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var expirationDate = DateTime.Now.AddMinutes(_options.ExpiryMinutes);
        var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials, expires: expirationDate);
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return encodedJwt;
    }

    public string ValidateToken(string token)
    {
        var principal = GetPrincipal(token);
        if (principal == null)
        {
            return string.Empty;
        }

        ClaimsIdentity identity;
        try
        {
            identity = (ClaimsIdentity)principal.Identity;
        }
        catch (NullReferenceException)
        {
            return string.Empty;
        }
        var userIdClaim = identity?.FindFirst("userId");
        if (userIdClaim == null)
        {
            return string.Empty;
        }
        var userId = userIdClaim.Value;
        return userId;
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            if (jwtToken == null)
            {
                return null;
            }
            var key = Encoding.UTF8.GetBytes(_options.Secret);
            var parameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            IdentityModelEventSource.ShowPII = true;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out _);
            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}