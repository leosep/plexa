using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Enums;
using Plexa.Infrastructure;
using Plexa.Infrastructure.Identity;
using Plexa.Web.Infrastructure;
using Plexa.Web.Models;

namespace Plexa.Web.Controllers;

[Authorize]
public sealed class SettingsController : Controller
{
    private readonly ISettingsService _settings;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IUserContext _user;
    private readonly PlexaDbContext _db;

    public SettingsController(ISettingsService settings, UserManager<ApplicationUser> users, IUserContext user, PlexaDbContext db)
    {
        _settings = settings;
        _users = users;
        _user = user;
        _db = db;
    }

    [HttpGet("/settings")]
    public async Task<IActionResult> Index(string section = "profile", CancellationToken ct = default)
    {
        var normalized = section is "preferences" or "mailbox" ? section : "profile";
        var model = new SettingsViewModel
        {
            Section = normalized,
            Profile = await _settings.GetProfileAsync(ct),
            Mailbox = await _settings.GetMailboxAsync(ct),
            Error = TempData["Error"] as string,
            Success = TempData["Success"] as string
        };

        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "settings-body")
        {
            return PartialView("_SettingsBody", model);
        }

        return View(model);
    }

    [HttpPost("/settings/profile/avatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile? avatar, CancellationToken ct)
    {
        if (avatar is null || avatar.Length == 0)
        {
            TempData["Error"] = "Choose an image first.";
            return Redirect("/settings?section=profile");
        }

        await using var stream = avatar.OpenReadStream();
        var result = await _settings.UpdateAvatarAsync(stream, avatar.FileName, avatar.ContentType, avatar.Length, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Profile picture updated." : result.Error;
        Response.Headers["HX-Trigger"] = "{\"profile:changed\":true}";
        return Redirect("/settings?section=profile");
    }

    [HttpPost("/settings/mailbox")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMailbox(MailboxDto model, string? password, CancellationToken ct)
    {
        var result = await _settings.SaveMailboxAsync(model.Email, model.SmtpHost, model.SmtpPort, model.UseStartTls, password, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok
            ? "Mailbox saved. The password is never shown again."
            : result.Error;
        Response.Headers["HX-Trigger"] = "{\"settings:changed\":true}";
        return Redirect("/settings?section=mailbox");
    }

    [HttpPost("/settings/preferences")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePreferences(bool showInsights, bool hideUpgrade, string? locale, CancellationToken ct)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == _user.UserId, ct);
        user.Locale = locale is "es" or "en" ? locale : user.Locale;
        await _db.SaveChangesAsync(ct);
        Response.Cookies.Append("plexa.preferences", $"insights={(showInsights ? 1 : 0)};upgrade={(hideUpgrade ? 1 : 0)}", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true
        });
        TempData["Success"] = "Preferences saved.";
        return Redirect("/settings?section=preferences");
    }
}

[Authorize(Policy = Policies.AdminOnly)]
public sealed class AdminController : Controller
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin)
    {
        _admin = admin;
    }

    [HttpGet("/admin")]
    public IActionResult Index() => Redirect("/admin/sprints");

    [HttpGet("/admin/sprints")]
    public async Task<IActionResult> Sprints(CancellationToken ct) =>
        View(new AdminSprintsViewModel { Sprints = await _admin.GetSprintsAsync(ct) });

    [HttpPost("/admin/sprints")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSprint(string name, DateOnly start, DateOnly end, int capacityPoints, int committedPoints, bool activate, CancellationToken ct)
    {
        var result = await _admin.CreateSprintAsync(name, start, end, capacityPoints, committedPoints, activate, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Sprint saved." : result.Error;
        Response.Headers["HX-Trigger"] = "{\"sprint:changed\":true}";
        return Redirect("/admin/sprints");
    }

    [HttpPost("/admin/sprints/{id:guid}/close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseSprint(Guid id, CancellationToken ct)
    {
        var result = await _admin.CloseSprintAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Sprint closed." : result.Error;
        Response.Headers["HX-Trigger"] = "{\"sprint:changed\":true}";
        return Redirect("/admin/sprints");
    }

    [HttpGet("/admin/members")]
    public async Task<IActionResult> Members(CancellationToken ct) =>
        View(new AdminMembersViewModel { Members = await _admin.GetMembersAsync(ct) });

    [HttpPost("/admin/members")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteMember(string email, WorkspaceRole role, string fullName, string jobTitle, decimal capacityHours, CancellationToken ct)
    {
        var result = await _admin.InviteMemberAsync(email, role, fullName, jobTitle, capacityHours, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Invitation sent." : result.Error;
        return Redirect("/admin/members");
    }

    [HttpPost("/admin/members/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMember(Guid id, string fullName, string jobTitle, WorkspaceRole role, decimal capacityHours, CancellationToken ct)
    {
        var result = await _admin.UpdateMemberAsync(id, fullName, jobTitle, role, capacityHours, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Member updated." : result.Error;
        return Redirect("/admin/members");
    }

    [HttpGet("/admin/projects")]
    public async Task<IActionResult> Projects(CancellationToken ct)
    {
        ViewData["Members"] = await _admin.GetMembersAsync(ct);
        return View(new AdminProjectsViewModel { Projects = await _admin.GetProjectsAsync(ct) });
    }

    [HttpPost("/admin/projects")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProject(AdminProjectFormModel model, CancellationToken ct)
    {
        var result = await _admin.SaveProjectAsync(model, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Project saved." : result.Error;
        Response.Headers["HX-Trigger"] = "{\"project:changed\":true}";
        return Redirect("/admin/projects");
    }

    [HttpPost("/admin/projects/{id:guid}/archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveProject(Guid id, CancellationToken ct)
    {
        var result = await _admin.ArchiveProjectAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "Project archived." : result.Error;
        return Redirect("/admin/projects");
    }
}
