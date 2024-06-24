using System;

namespace Middleware.Model
{
    public class SendMessage
    {
        public required Guid ToUser { get; set; }
        public required string Text { get; set; }
    }
}
