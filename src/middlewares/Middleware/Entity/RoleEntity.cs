using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Entity
{
    [Index(nameof(Name), IsUnique = true)]
    public class RoleEntity
    {
        public RoleEntity()
        {
            Users = new HashSet<UserEntity>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        public virtual ICollection<UserEntity> Users { get; set; }
    }
}
