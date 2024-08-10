using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ForagerSite.Data;
using ForagerSite.Models;


namespace DatabaseTesting
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<ForagerDbContext>(options =>
                    options.UseSqlServer("Server=LITTLEBEAR\\SQLEXPRESS;Database=ForagerDB;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"))
                .BuildServiceProvider();

            // Get DbContext instance
            using (var context = serviceProvider.GetService<ForagerDbContext>())
            {
                // Create a new user
                var newUser = new User
                {
                    UsrName = "BilboBaggins4Lyfe",
                    UsrBio = "A new user bio",
                    UsrEmail = "Bilbo.Baggins@example.com",
                    UsrFindsNum = 0,
                    UsrExpScore = 0,
                    UsrJoinedDate = DateTime.Now,
                    UsrCountry = "USA",
                    UsrStateorProvince = "CA",
                    UsrZipCode = 12335
                };

                // Add the new user to the context
                context.Users.Add(newUser);

                // Save changes to the database
                context.SaveChanges();

                // Confirm the user was added
                var usersCount = context.Users.Count();
                Console.WriteLine($"Number of users in the database: {usersCount}");
                //foreach (var user in context.Users)
                //    Console.WriteLine("");

            }
        }
    }
}
