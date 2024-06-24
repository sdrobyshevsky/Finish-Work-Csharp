using Middleware.Model;

namespace Middleware
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserService userService;
        private readonly TokenService tokenService;

        public AuthenticationService(UserService userService, TokenService tokenService)
        {
            this.userService = userService;
            this.tokenService = tokenService;
        }

        public string Authenticate(UserCredentials userCredentials)
        {
            userService.ValidateCredentials(userCredentials);
            string securityToken = tokenService.GetToken(userCredentials.Email);

            return securityToken;
        }
    }
}
