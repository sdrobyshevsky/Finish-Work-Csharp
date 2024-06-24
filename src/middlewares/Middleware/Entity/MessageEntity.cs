using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Entity
{
    public class MessageEntity
    {
        public int Id { get; set; }

        [Required]
        public virtual UserEntity FromUser { get; set; }
        public Guid FromUserId { get; set; }

        [Required]
        public virtual UserEntity ToUser { get; set; }
        public Guid ToUserId { get; set; }
        public required string Text { get; set; }
        public bool IsRead { get; set; }
        public DateTime SendDate { get; set; }
    }
}
