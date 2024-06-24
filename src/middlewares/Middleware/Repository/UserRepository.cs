using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Middleware;
using Middleware.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityMicroservice.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext userDbContext;
    private readonly IMapper mapper;
    public UserRepository(UserDbContext dbContext, IMapper mapper)
    {
        this.userDbContext = dbContext;
        this.mapper = mapper;
    }

    public void DeleteUser(Guid userId)
    {
        var user = userDbContext.Users.Find(userId);
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User not found");
        }

        userDbContext.Users.Remove(user);
        userDbContext.SaveChanges();
    }

    public User FindUser(Guid userId)
    {
        var user = userDbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);

        if (user == null) return null;

        var result = mapper.Map<User>(user);

        return result;
    }

    public User? GetUser(string email)
    {
        var user = userDbContext.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Email == email);

        if (user == null) return null;

        var result = mapper.Map<User>(user);

        return result;
    }

    public List<User> GetUsers()
    {
        var allUsers = userDbContext.Users.Include(u => u.Role).ToList();

        var result = mapper.Map<List<User>>(allUsers);

        return result;
    }

    public void InsertUser(User user)
    {
        var newUser = mapper.Map<UserEntity>(user);

        userDbContext.Users.Add(newUser);
        userDbContext.SaveChanges();
    }

    public void UpdateUser(User user)
    {
        var existingUser = userDbContext.Users.Single(u=>u.Id  == user.Id);

        existingUser.Email = user.Email;
        existingUser.Name = user.Name;
        existingUser.Password = user.Password;
        existingUser.Salt = user.Salt;
        existingUser.RoleId = user.IsAdmin ? 1 : 2;
    }
}