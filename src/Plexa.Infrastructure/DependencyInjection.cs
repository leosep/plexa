using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plexa.Application.Abstractions;
using Plexa.Application.Services;
using Plexa.Infrastructure.Identity;
using Plexa.Infrastructure.Services;

namespace Plexa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlexaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? configuration["ConnectionStrings__Default"]
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión. Define ConnectionStrings__Default (variable de entorno o user-secrets).");

        services.AddDbContext<PlexaDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                sql.CommandTimeout(60);
                sql.MigrationsAssembly(typeof(PlexaDbContext).Assembly.GetName().Name);
                // SQL Server 2012 (compat 110) no soporta OPENJSON (2016+). Con un nivel
                // de compatibilidad inferior EF Core traduce Contains a IN/VALUES.
                sql.UseCompatibilityLevel(110);
            }));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<PlexaDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/error/403";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.Cookie.Name = "plexa.auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        });

        // Claves persistentes: sin esto las cookies y los secretos SMTP cifrados
        // se invalidan al reiniciar (y en contenedores se pierden por completo).
        var keysPath = configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(keysPath))
        {
            keysPath = Path.Combine(AppContext.BaseDirectory, "storage", "keys");
        }

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .SetApplicationName("Plexa");

        services.AddHttpContextAccessor();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<ISecretProtector, DataProtectionSecretProtector>();
        services.AddScoped<IMailer, SmtpMailer>();

        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITimerService, TimerService>();
        services.AddScoped<ICapacityService, CapacityService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        return services;
    }
}
