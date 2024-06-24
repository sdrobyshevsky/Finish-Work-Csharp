using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware;
using System;
using System.Linq;
using System.Security.Claims;

namespace IdentityMicroservice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController(IUserRepository userRepository, 
    IAuthenticationService authenticationService, IEncryptor encryptor)
    : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] User user, [FromQuery(Name = "d")] string destination = "frontend")
    {
        var u = userRepository.GetUser(user.Email);

        if (u == null)
        {
            return NotFound("User not found.");
        }

        if (destination == "backend" && !u.IsAdmin)
        {
            return BadRequest("Could not authenticate user.");
        }

        var isValid = u.ValidatePassword(user.Password, encryptor);

        if (!isValid)
        {
            return BadRequest("Could not authenticate user.");
        }

        var token = authenticationService.Authenticate(
            new Middleware.Model.UserCredentials
            {
                Email = user.Email,
                Password = user.Password
            });

        return Ok(token);
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        var u = userRepository.GetUser(user.Email);

        if (u != null)
        {
            return BadRequest("User already exists.");
        }

        user.SetPassword(user.Password, encryptor);
        
        userRepository.InsertUser(user);

        return Ok();
    }

    [HttpDelete("user")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteUser([FromQuery(Name = "userId")] Guid userId)
    {
        if (User.HasClaim(c => c.Type == ClaimTypes.PrimarySid && Guid.Parse(c.Value) == userId))
        {
            return BadRequest("Admin cannot delete himself.");
        }

        try
        {
            userRepository.DeleteUser(userId);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ex.Message);
        }        

        return Ok();
    }

    [HttpGet("users")]
    public IActionResult Users()
    {
        var users = userRepository.GetUsers();

        return Ok(users);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.Claims.First(c => c.Type.Equals(ClaimTypes.PrimarySid));
        return Ok(userId.Value);
    }
}