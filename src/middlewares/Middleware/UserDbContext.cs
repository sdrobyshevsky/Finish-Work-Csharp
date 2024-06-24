using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Middleware.Entity;
using System;

namespace Middleware
{
    public class UserDbContext: DbContext
    {
        private readonly IEncryptor encryptor;

        public UserDbContext(DbContextOptions<UserDbContext> options, IEncryptor encryptor)
        : base(options) 
        {
            this.encryptor = encryptor;
        }

        public virtual DbSet<RoleEntity> Roles { get; set; }
        public virtual DbSet<UserEntity> Users { get; set; }
        public virtual DbSet<MessageEntity> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleEntity>()
                .HasMany(e => e.Users)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);

            modelBuilder.Entity<UserEntity>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<MessageEntity>(entity => 
            {
                entity.HasKey(n => n.Id);

                entity.HasOne(n => n.FromUser)
                    .WithMany(u => u.RecievedMessages)
                    .HasForeignKey(n => n.FromUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.ToUser)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(n => n.ToUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
 
            Seed(modelBuilder);
        }

        private void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleEntity>().HasData(
                new RoleEntity { Id = 1, Name = "Admin" },
                new RoleEntity { Id = 2, Name = "User" });

            var adminSalt = encryptor.GetSalt();
            var adminPassword = encryptor.GetHash("123@46Ms", adminSalt);

            modelBuilder.Entity<UserEntity>().HasData(
               new UserEntity 
               { 
                   Id = Guid.Parse("10dcaaee-f2c0-41a9-993a-c3f544964b7c"), 
                   Name = "Admin",
                   Email = "admin2@mail.com",
                   RoleId = 1,
                   Salt = adminSalt,
                   Password = adminPassword
               });
        }
    }
}
