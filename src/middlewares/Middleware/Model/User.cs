using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Middleware;

public class User
{
    public Guid Id { get; init; }
    [EmailAddress]
    public required string Email { get; init; }
    [MinLength(6)]
    public required string Name { get; init; }
    [MinLength(6)]
    [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$")]
    public required string Password { get; set; } 
    public string? Salt { get; set; }
    public bool IsAdmin { get; init; }

    public IEnumerable<string> Roles { get; set; }

    public IEnumerable<Claim> Claims()
    {
        var claims = new List<Claim> {
            new(ClaimTypes.Name, Name),
            new (ClaimTypes.Email, Email),
            new (ClaimTypes.PrimarySid, Id.ToString()),
        };
        claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    public void SetPassword(string password, IEncryptor encryptor)
    {
        Salt = encryptor.GetSalt();
        Password = encryptor.GetHash(password, Salt);
    }

    public bool ValidatePassword(string password, IEncryptor encryptor) =>
        Password == encryptor.GetHash(password, Salt);
}