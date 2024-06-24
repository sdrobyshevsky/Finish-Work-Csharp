using AutoMapper;
using Middleware.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Middleware;

public class MessageRepository : IMessageRepository
{
    private readonly UserDbContext userDbContext;
    private readonly IMapper mapper;

    public MessageRepository(UserDbContext dbContext, IMapper mapper)
    {
        this.userDbContext = dbContext;
        this.mapper = mapper;
    }

    public IList<GetMessage> GetMessages(Guid userId)
    {
        var messages = userDbContext.Messages.Where(m => !m.IsRead && m.ToUserId == userId).ToList();

        messages.ForEach(m => m.IsRead = true);

        userDbContext.UpdateRange(messages);
        userDbContext.SaveChanges();

        var result = mapper.Map<List<GetMessage>>(messages);

        return result;
    }

    public GetMessage InsertMessage(GetMessage message)
    {
        var newMessage = mapper.Map<MessageEntity>(message);

        userDbContext.Messages.Add(newMessage);
        userDbContext.SaveChanges();

        var result = mapper.Map<GetMessage>(newMessage);

        return message;
    }
}