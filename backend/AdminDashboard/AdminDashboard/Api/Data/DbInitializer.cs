using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (await userManager.FindByEmailAsync("admin@mirra.dev") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@mirra.dev",
                    Email = "admin@mirra.dev",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "admin123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!context.Tags.Any())
            {
                var tags = new List<Tag>
                {
                    new Tag { Name = "VIP", Color = "#FF5733" },
                    new Tag { Name = "New", Color = "#33FF57" },
                    new Tag { Name = "Inactive", Color = "#3357FF" }
                };

                await context.Tags.AddRangeAsync(tags);
                await context.SaveChangesAsync();
            }

            if (!context.Clients.Any())
            {
                var tags = await context.Tags.ToListAsync();

                var clients = new List<Client>
                {
                    new Client
                    {
                        Id = 1,
                        Name = "John Doe",
                        Email = "john@example.com",
                        Balance = 1000,
                        Tags = new List<Tag> { tags[0], tags[1] }
                    },
                    new Client
                    {
                        Id = 2,
                        Name = "Jane Smith",
                        Email = "jane@example.com",
                        Balance = 2000,
                        Tags = new List<Tag> { tags[0] }
                    },
                    new Client
                    {
                        Id = 3,
                        Name = "Bob Johnson",
                        Email = "bob@example.com",
                        Balance = 1500,
                        Tags = new List<Tag> { tags[2] }
                    }
                };

                await context.Clients.AddRangeAsync(clients);
                await context.SaveChangesAsync();
            }

            if (!context.Payments.Any())
            {
                var clients = await context.Clients.ToListAsync();
                var random = new Random();
                var payments = new List<Payment>();

                for (int i = 0; i < 5; i++)
                {
                    var client = clients[random.Next(clients.Count)];
                    payments.Add(new Payment
                    {
                        ClientId = client.Id,
                        Amount = random.Next(100, 1000),
                        CreatedAt = DateTime.UtcNow.AddDays(-i)
                    });
                }

                await context.Payments.AddRangeAsync(payments);
                await context.SaveChangesAsync();
            }

            if (!context.Rates.Any())
            {
                await context.Rates.AddAsync(new Rate { Value = 10 });
                await context.SaveChangesAsync();
            }
        }
    }
}