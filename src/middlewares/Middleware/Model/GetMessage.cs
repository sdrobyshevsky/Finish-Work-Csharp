using Middleware.Model;
using System;

namespace Middleware;

public class GetMessage : SendMessage
{
    public int Id { get; set; }
    public required Guid FromUser { get; set; }
    public bool IsRead { get; set; }
    public DateTime SendDate { get; set; }
}