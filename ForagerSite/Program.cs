using ForagerSite.Services;
using ForagerSite.Components;
using DataAccess.Data;
using ForagerSite.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ForagerSite.Services.Interfaces;
using ForagerSite.Features.Finds;
using ForagerSite.Features.Map;
using ForagerSite.Features.Media;

namespace ForagerSite
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("Default")
                ?? throw new NullReferenceException("No connection string in config");

            builder.Services.AddDbContextFactory<ForagerDbContext>((DbContextOptionsBuilder options) =>
                options.UseSqlServer(connectionString));

            builder.Services.AddAuthorizationCore();
            builder.Services.AddRazorComponents().
                AddInteractiveServerComponents()
                .AddCircuitOptions(options =>
                {
                    options.DetailedErrors = true;
                });
            builder.Services.AddSingleton<LocationIndexService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<FindQueries>();
            builder.Services.AddScoped<FindCommands>();
            builder.Services.AddScoped<FindInteractions>();
            builder.Services.AddScoped<MapFindQueries>();
            builder.Services.AddScoped<IPhotoStorage, LocalPhotoStorage>();
            builder.Services.AddScoped<PhotoService>();
            builder.Services.AddScoped<IMapService, MapService>();
            builder.Services.AddScoped<IFriendService, FriendService>();
            builder.Services.AddScoped<IUserSessionService, UserSessionService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IInboxService, InboxService>();
            builder.Services.AddScoped<ProtectedSessionStorage>();
            



            //builder.Services.AddTransient<EmailService>();
            //builder.Services.AddTransient<PasswordResetService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var geo = scope.ServiceProvider.GetRequiredService<LocationIndexService>();
                Task.Run(() => geo.InitializeAsync()).GetAwaiter().GetResult();
            }



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();


            app.Run();
        }

    }
}
