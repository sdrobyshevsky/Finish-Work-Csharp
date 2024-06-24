using MessageMicroservice.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Middleware;
using Moq;
using System.Security.Claims;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace MessageMicroservice.UnitTests;

public class MessageControllerTest
{
    private readonly MessageController _controller;
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

    private readonly List<GetMessage> _messages = new()
    {
        new()
        {
            FromUser = AdminUserId,
            ToUser = FrontendUserId,
            SendDate = DateTime.Now,
            Text = "Hey test",
            IsRead = true,
            Id = 1
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

    public MessageControllerTest()
    {
        var mockMessageRepo = new Mock<IMessageRepository>();
        mockMessageRepo.Setup(repo => repo.GetMessages(It.IsAny<Guid>()))
            .Returns<Guid>(x => _messages);
        mockMessageRepo.Setup(repo => repo.InsertMessage(It.IsAny<GetMessage>()));

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(repo => repo.GetUser(It.IsAny<string>()))
            .Returns<string>(email => _users.FirstOrDefault(u => u.Email == email));
        mockUserRepo.Setup(repo => repo.FindUser(It.IsAny<Guid>()))
            .Returns<Guid>(id => _users.FirstOrDefault(u => u.Id == id));
        mockUserRepo.Setup(repo => repo.InsertUser(It.IsAny<User>()))
            .Callback<User>(_users.Add);
        var configuration = InitConfiguration();

        _controller = new MessageController(mockMessageRepo.Object, mockUserRepo.Object);

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
                    new Claim(ClaimTypes.PrimarySid, _users[0].Id.ToString())
                ]))
        };

        _controller.ControllerContext.HttpContext = context;
    }

    [Fact]
    public void SendMessageTest()
    {
        var okObjectResult = _controller.Post(_messages[0]);
        var okResult = Assert.IsType<CreatedAtActionResult>(okObjectResult);
        var message = Assert.IsType<GetMessage>(okResult.Value);
        Assert.NotNull(message);
        Assert.Equal(message.Text, _messages[0].Text);
    }

    [Fact]
    public void GetMessagesTest()
    {
        // Success
        var okObjectResult = _controller.Get();
        var okResult = Assert.IsType<OkObjectResult>(okObjectResult);
        var messages = okResult.Value as List<GetMessage>;
        Assert.NotEmpty(messages);
    }
}