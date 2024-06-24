using Middleware.Model;

namespace Middleware
{
    public interface IAuthenticationService
    {
        string Authenticate(UserCredentials userCredentials);
    }
}
