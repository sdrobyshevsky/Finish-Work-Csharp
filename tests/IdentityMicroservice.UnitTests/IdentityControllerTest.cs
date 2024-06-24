using IdentityMicroservice.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Middleware;
using Moq;
using System.Security.Claims;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace IdentityMicroservice.UnitTests;

public class IdentityControllerTest
{
    private readonly IdentityController _controller;
    private static readonly Guid AdminUserId = Guid.Parse("d7ea83ea-630f-49db-aa84-7a66e3f68aee");
    private static readonly Guid FrontendUserId = Guid.Parse("7273c879-f473-4524-8527-215ebacf9690");
    private static readonly User UnknownUser = new()
    {
        Id = Guid.Parse("b20c535f-e0f7-495a-8c37-5dff0ec94c64"),
        Email = "unknown@store.com",
        Name = "Unknown",
        Password = "F3+cR6hU2F5DdDi14nx89azQkUhgqF2regIO/ySGcjSbuuzQiaBjVg==",
        Salt = "tDYGGkF69/UfbY0oxPTFLgmttn+MCWI3ZmSHMAOd3zVwCDzKzW7n+w==",
        IsAdmin = false
    };
    private readonly List<User> _users = new()
    {
        new()
        {
            Id = AdminUserId,
            Email = "admin@store.com",
            Name = "Admin",
            Password = "F3+cR6hU2F5DdDi14nx89azQkUhgqF2regIO/ySGcjSbuuzQiaBjVg==",
            Salt = "tDYGGkF69/UfbY0oxPTFLgmttn+MCWI3ZmSHMAOd3zVwCDzKzW7n+w==",
            IsAdmin = true,
            Roles = ["Admin"]
        },
        new()
        {
            Id = FrontendUserId,
            Email = "jdoe@store.com",
            Name = "Front User",
            Password = "F3+cR6hU2F5DdDi14nx89azQkUhgqF2regIO/ySGcjSbuuzQiaBjVg==",
            Salt = "tDYGGkF69/UfbY0oxPTFLgmttn+MCWI3ZmSHMAOd3zVwCDzKzW7n+w==",
            IsAdmin = false,
            Roles = ["User"]
        }
    };

    private static IConfiguration InitConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        return config;
    }

    public IdentityControllerTest()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(repo => repo.GetUser(It.IsAny<string>()))
            .Returns<string>(email => _users.FirstOrDefault(u => u.Email == email));
        mockRepo.Setup(repo => repo.InsertUser(It.IsAny<User>()))
            .Callback<User>(_users.Add);
        var configuration = InitConfiguration();
        var encryptor = new Encryptor();
        var userService = new UserService(mockRepo.Object, encryptor);
        var tokenService = new TokenService(mockRepo.Object);

        _controller = new IdentityController(mockRepo.Object, new AuthenticationService(userService, tokenService), new Encryptor());

        _controller.ControllerContext = new ControllerContext();

        var response = new HttpResponseFeature();

        var features = new FeatureCollection();
        features.Set<IHttpResponseFeature>(response);

        var context = new DefaultHttpContext(features)
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "<username>"),
                    new Claim(ClaimTypes.Role, "<role>"),
                    new Claim(ClaimTypes.PrimarySid, _users[1].Id.ToString())
                ]))
        };

        _controller.ControllerContext.HttpContext = context;
    }

    [Fact]
    public void LoginTest()
    {
        // User not found
        var notFoundObjectResult = _controller.Login(UnknownUser);
        Assert.IsType<NotFoundObjectResult>(notFoundObjectResult);

        // Backend failure
        var user = new User
        {
            Id = FrontendUserId,
            Email = "jdoe@store.com",
            Name = "Front user",
            Password = "aaaaaa",
            IsAdmin = false
        };
        var badRequestObjectResult = _controller.Login(user, "backend");
        Assert.IsType<BadRequestObjectResult>(badRequestObjectResult);

        // Wrong password
        user.Password = "bbbbbb";
        badRequestObjectResult = _controller.Login(user);
        Assert.IsType<BadRequestObjectResult>(badRequestObjectResult);

        // Frontend success
        user.Password = "123@46Ms";
        var okObjectResult = _controller.Login(user);
        var okResult = Assert.IsType<OkObjectResult>(okObjectResult);
        var token = Assert.IsType<string>(okResult.Value);
        Assert.NotEmpty(token);

        // Backend success
        var adminUser = new User
        {
            Id = AdminUserId,
            Email = "admin@store.com",
            Name = "Admin",
            Password = "123@46Ms",
            IsAdmin = true,
            Roles = ["Admin"]
        };
        okObjectResult = _controller.Login(adminUser, "backend");
        okResult = Assert.IsType<OkObjectResult>(okObjectResult);
        token = Assert.IsType<string>(okResult.Value);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void RegisterTest()
    {
        // Failure (user already exists)
        var user = new User
        {
            Id = FrontendUserId,
            Email = "jdoe@store.com",
            Name = "Front user",
            Password = "aa@Aaa3aa",
            IsAdmin = false
        };
        var badRequestObjectResult = _controller.Register(user);
        Assert.IsType<BadRequestObjectResult>(badRequestObjectResult);

        // Success (new user)
        user = new User
        {
            Id = Guid.Parse("8737330a-5996-44fd-84c2-a9b4ea155d04"),
            Email = "ctaylor@store.com",
            Name = "Front user",
            Password = "ccc@c2Acc",
            IsAdmin = false
        };
        var okResult = _controller.Register(user);
        Assert.IsType<OkResult>(okResult);
        Assert.NotNull(_users.FirstOrDefault(u => u.Id == user.Id));
    }

    [Fact]
    public void MeTest()
    {
        // Success
        var okObjectResult = _controller.Me();
        var okResult = Assert.IsType<OkObjectResult>(okObjectResult);
        Assert.Equal(FrontendUserId.ToString(), okResult.Value.ToString());
    }
}