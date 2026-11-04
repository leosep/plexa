using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Infrastructure;
using Plexa.Infrastructure.Identity;
using Plexa.Infrastructure.Seed;
using Plexa.Web.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/plexa-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14));

builder.Services.AddPlexaInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy => policy.RequireClaim(PlexaClaims.Role, "Admin"));
    options.AddPolicy(Policies.Authenticated, policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddScoped<IAuthorizationHandler, ProjectAccessHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.ProjectMember, policy => policy.Requirements.Add(new ProjectRequirement(ProjectAccessLevel.Member)));
    options.AddPolicy(Policies.ProjectLeadOrAdmin, policy => policy.Requirements.Add(new ProjectRequirement(ProjectAccessLevel.Lead)));
});

builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddScoped<IAppLinkGenerator, AppLinkGenerator>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, PlexaClaimsFactory>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { new CultureInfo("en"), new CultureInfo("es") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider { CookieName = "plexa.culture" },
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddHealthChecks().AddDbContextCheck<PlexaDbContext>();
builder.Services.AddResponseCompression();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

var app = builder.Build();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});

app.UseRouting();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = services.GetRequiredService<PlexaDbContext>();

    // En Development se prepara todo automáticamente; en otros entornos se opta
    // explícitamente con Database__MigrateOnStartup / Database__SeedOnStartup.
    var migrateOnStartup = builder.Configuration.GetValue(
        "Database:MigrateOnStartup",
        app.Environment.IsDevelopment());
    var seedOnStartup = builder.Configuration.GetValue(
        "Database:SeedOnStartup",
        app.Environment.IsDevelopment());
    var seedOnly = args.Contains("--seed");

    if (migrateOnStartup || seedOnly)
    {
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudieron aplicar las migraciones al arrancar.");
        }
    }

    if (seedOnStartup || seedOnly)
    {
        try
        {
            await SeedData.RunAsync(services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo ejecutar el seed de datos.");
        }
    }

    if (seedOnly)
    {
        return;
    }
}

app.Run();

public partial class Program;
