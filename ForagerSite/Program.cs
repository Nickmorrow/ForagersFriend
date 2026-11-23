using ForagerSite.Services;
using ForagerSite.Components;
using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ForagerSite.Services.Interfaces;
using ForagerSite.Features.Finds;
using ForagerSite.Features.Map;
using ForagerSite.Features.Media;
using System.Threading.RateLimiting;
using System.Net;
using System.Globalization;

namespace ForagerSite
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("Default")
                ?? throw new NullReferenceException("No connection string in config");
            var requireConfirmedEmail = !builder.Environment.IsDevelopment() ||
                builder.Configuration.GetValue<bool>("Authentication:RequireConfirmedEmail");

            builder.Services.AddDbContextFactory<ForagerDbContext>((DbContextOptionsBuilder options) =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentityCore<ForagerAccount>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                options.Lockout.AllowedForNewUsers = false;
            })
                .AddEntityFrameworkStores<ForagerDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromHours(1));

            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
                options.Cookie.Name = "ForagersFriend.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = false;
            });
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromMinutes(5));
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IAccountEmailSender, SmtpAccountEmailSender>();
            builder.Services.AddScoped<IAccountEmailQueue, AccountEmailQueue>();
            builder.Services.AddSingleton<AccountRecoveryRateLimiter>();
            builder.Services.AddHostedService<AccountEmailWorker>();
            var trustedProxyAddresses = builder.Configuration
                .GetSection("ForwardedHeaders:KnownProxies")
                .Get<string[]>() ?? Array.Empty<string>();
            var forwardedHeaderLimit = builder.Configuration.GetValue("ForwardedHeaders:ForwardLimit", 1);
            if (forwardedHeaderLimit < 1)
                throw new InvalidOperationException("ForwardedHeaders:ForwardLimit must be greater than zero.");

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = forwardedHeaderLimit;
                foreach (var address in trustedProxyAddresses)
                {
                    if (!IPAddress.TryParse(address, out var proxyAddress))
                        throw new InvalidOperationException($"'{address}' is not a valid forwarded-header proxy IP address.");

                    options.KnownProxies.Add(proxyAddress);
                }
            });
            var dataProtection = builder.Services.AddDataProtection()
                .SetApplicationName("ForagersFriend");
            var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
            if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
            {
                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
            }
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = (context, _) =>
                {
                    var httpContext = context.HttpContext;
                    var policyName = httpContext.GetEndpoint()?.Metadata
                        .GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;

                    if (policyName == "account-registration")
                    {
                        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                            ? retry
                            : TimeSpan.FromMinutes(10);
                        var retrySeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                        var retryUntil = DateTimeOffset.UtcNow.AddSeconds(retrySeconds).ToUnixTimeSeconds();
                        httpContext.Response.Headers["Retry-After"] = retrySeconds.ToString(CultureInfo.InvariantCulture);
                        httpContext.Response.Redirect($"/Registration?rateLimitUntil={retryUntil}");
                    }
                    else if (policyName == "account-login")
                    {
                        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                            ? retry
                            : TimeSpan.FromMinutes(1);
                        httpContext.Response.Headers["Retry-After"] = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
                        httpContext.Response.Redirect("/Login?status=rate-limited");
                    }
                    else
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    }

                    return ValueTask.CompletedTask;
                };
                options.AddPolicy("account-login", context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
                options.AddPolicy("account-recovery", context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
                options.AddPolicy("account-registration", context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            });
            builder.Services.AddRazorComponents().
                AddInteractiveServerComponents()
                .AddCircuitOptions(options =>
                {
                    options.DetailedErrors = builder.Environment.IsDevelopment();
                });
            builder.Services.AddSingleton<LocationIndexService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
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
            




            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var geo = scope.ServiceProvider.GetRequiredService<LocationIndexService>();
                Task.Run(() => geo.InitializeAsync()).GetAwaiter().GetResult();
            }



            // Configure the HTTP request pipeline.
            app.UseForwardedHeaders();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();


            app.Run();
        }

    }
}
