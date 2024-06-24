using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware;
using System.Linq;
using System.Security.Claims;
using System;
using Middleware.Model;

namespace MessageMicroservice.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MessageController(IMessageRepository messageRepository, IUserRepository userRepository) : ControllerBase
{
    // GET: api/<MessageController>
    [HttpGet]
    public IActionResult Get()
    {
        var userId = Guid.Parse(User.Claims.First(c => c.Type.Equals(ClaimTypes.PrimarySid)).Value);
        var messages = messageRepository.GetMessages(userId);
        return Ok(messages);
    }

    // POST api/<MessageController>
    [HttpPost]
    public IActionResult Post([FromBody] SendMessage message)
    {
        var toUser = userRepository.FindUser(message.ToUser);
        if (toUser == null)
        {
            return BadRequest($"{nameof(SendMessage.ToUser)} not found");
        }

        var userId = Guid.Parse(User.Claims.First(c => c.Type.Equals(ClaimTypes.PrimarySid)).Value);
        var newMessage = new GetMessage
        { 
            FromUser = userId,
            Text = message.Text,
            ToUser = message.ToUser
        };
        messageRepository.InsertMessage(newMessage);
        return CreatedAtAction(nameof(Post), newMessage);
    }
}