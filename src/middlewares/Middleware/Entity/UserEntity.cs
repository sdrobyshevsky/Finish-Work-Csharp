using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Entity
{
    [Index(nameof(Email), nameof(Name), IsUnique = true)]
    public class UserEntity
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 6)]

        public string Password { get; set; }
        public string Salt { get; set; }

        public int RoleId { get; set; }

        public virtual RoleEntity Role { get; set; }

        public virtual ICollection<MessageEntity> RecievedMessages { get; set; }
        public virtual ICollection<MessageEntity> SentMessages { get; set; }
    }
}
