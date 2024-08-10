using ForagerSite.Components;
using ForagerSite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace ForagerSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("Default")
                ??throw new NullReferenceException("No connection string in config");
            
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            //builder.Services.AddDbContext<ForagerDbContext>(options =>
            //    options.UseSqlServer("Server=LITTLEBEAR\\SQLEXPRESS;Database=ForagerDB;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"));

            builder.Services.AddDbContextFactory<ForagerDbContext>((DbContextOptionsBuilder options) =>
                options.UseSqlServer(connectionString));

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
