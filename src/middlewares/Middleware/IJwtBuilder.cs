using System;

namespace Middleware;

public interface IJwtBuilder
{
    string GetToken(Guid userId);
    string ValidateToken(string token);
}