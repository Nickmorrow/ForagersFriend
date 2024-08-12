using ForagerWebsite.Components;
using ForagerWebSite.Data;
using ForagerWebSite.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoaragerWebsite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("Default")
                ?? throw new NullReferenceException("No connection string in config");

            builder.Services.AddDbContextFactory<ForagerDbContext>((DbContextOptionsBuilder options) =>
                options.UseSqlServer(connectionString));

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddControllers();
            builder.Services.AddTransient<UserService>();
            builder.Services.AddTransient<EmailService>();
            builder.Services.AddTransient<PasswordResetService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
