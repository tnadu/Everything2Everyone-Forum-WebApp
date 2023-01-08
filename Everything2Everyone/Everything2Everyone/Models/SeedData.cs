using Everything2Everyone.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Everything2Everyone.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any())
                {
                    return;
                }

                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Administrator", NormalizedName = "Administrator".ToUpper() },
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Editor", NormalizedName = "Editor".ToUpper() },
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "User".ToUpper() }
                    );
                
                var hasher = new PasswordHasher<User>();
                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange(
                new User
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    // primary key
                    UserName = "dragonul490@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "DRAGONUL490@TEST.COM",
                    Email = "dragonul490@test.com",
                    NormalizedUserName = "DRAGONUL490@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "test123!"),
                    FirstName = "Michael",
                    LastName = "Dover",
                    ShowPublicIdentity = false,
                    NickName = "ben.dover",
                    JoinDate = DateTime.Now
                },
                new User
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    // primary key
                    UserName = "magnesium7356@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "MAGNESIUM7356@TEST.COM",
                    Email = "magnesium7356@test.com",
                    NormalizedUserName = "MAGNESIUM7356@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "QKE6KUhW^CKi$B!Qm8dNhKXQ@Ex*k^"),
                    FirstName = "Jonathan",
                    LastName = "Thomason",
                    ShowPublicIdentity = false,
                    NickName = "j.tom",
                    JoinDate = DateTime.Now
                }
                );

                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    },
                    new IdentityUserRole<string>
                    {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

