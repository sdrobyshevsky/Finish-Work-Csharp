using Middleware.Model;

namespace Middleware;

public class UserService
{
    private readonly IUserRepository userRepository;
    private readonly IEncryptor encryptor;

    public UserService(IUserRepository userRepository, IEncryptor encryptor)
    {
        this.userRepository = userRepository;
        this.encryptor = encryptor;
    }

    public void ValidateCredentials(UserCredentials userCredentials)
    {
        User user = userRepository.GetUser(userCredentials.Email);
        bool isValid = user != null && AreValidCredentials(userCredentials, user);

        if (!isValid)
        {
            throw new InvalidCredentialsException();
        }
    }

    private bool AreValidCredentials(UserCredentials userCredentials, User user)
    {
        return user.Email == userCredentials.Email &&
               user.ValidatePassword(userCredentials.Password, encryptor);
    }
}
