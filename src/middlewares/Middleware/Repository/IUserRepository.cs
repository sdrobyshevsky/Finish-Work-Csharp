using System;
using System.Collections.Generic;

namespace Middleware;

public interface IUserRepository
{
    User? GetUser(string email);
    User? FindUser(Guid userId);
    List<User> GetUsers();

    void InsertUser(User user);
    void UpdateUser(User user);
    void DeleteUser(Guid userId);
}