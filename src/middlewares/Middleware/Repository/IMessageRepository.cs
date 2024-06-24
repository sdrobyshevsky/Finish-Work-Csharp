using System;
using System.Collections.Generic;

namespace Middleware;

public interface IMessageRepository
{
    IList<GetMessage> GetMessages(Guid userId);
    GetMessage InsertMessage(GetMessage message);
}