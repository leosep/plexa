using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Domain.Enums;
using Plexa.Infrastructure;
using Plexa.Infrastructure.Identity;
using Plexa.Web.Infrastructure;
using Plexa.Web.Models;

namespace Plexa.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    private readonly PlexaDbContext _db;
    private readonly IMailer _mailer;
    private readonly IAppLinkGenerator _links;
    private readonly IClock _clock;
    private readonly ILogger<AccountController> _logger;
    private readonly ITranslator _t;

    public AccountController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser> users,
        PlexaDbContext db,
        IMailer mailer,
        IAppLinkGenerator links,
        IClock clock,
        ILogger<AccountController> logger,
        ITranslator translator)
    {
        _signIn = signIn;
        _users = users;
        _db = db;
        _mailer = mailer;
        _links = links;
        _clock = clock;
        _logger = logger;
        _t = translator;
    }

    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _users.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, _t["We could not find an account with that email."]);
            return View(model);
        }

        var result = await _signIn.PasswordSignInAsync(user, model.Password, model.KeepSignedIn, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("Login succeeded for {Email}", model.Email);
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, _t["Too many attempts. Try again in 15 minutes."]);
        }
        else
        {
            ModelState.AddModelError(string.Empty, _t["The password is not correct."]);
        }

        return View(model);
    }

    [HttpPost("/logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet("/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous]
    [HttpPost("/forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _users.FindByEmailAsync(model.Email.Trim());
        if (user is not null)
        {
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var link = _links.AbsoluteUrl($"/reset-password?email={Uri.EscapeDataString(model.Email.Trim())}&code={Uri.EscapeDataString(token)}");
            await _mailer.SendAsync(model.Email.Trim(), _t["Mail.ResetSubject"],
                _t.Format("Mail.ResetBody", user.FullName, link), ct);
        }

        // Respuesta genérica: nunca revelamos si el correo existe.
        return View("ForgotPasswordSent", model.Email);
    }

    [AllowAnonymous]
    [HttpGet("/reset-password")]
    public IActionResult ResetPassword(string email, string code) =>
        View(new ResetPasswordViewModel { Email = email, Code = code });

    [AllowAnonymous]
    [HttpPost("/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _users.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, _t["That reset link is not valid anymore."]);
            return View(model);
        }

        var result = await _users.ResetPasswordAsync(user, model.Code, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, _t[error.Description]);
            }

            return View(model);
        }

        TempData["Toast"] = _t["Password updated. Sign in with your new password."];
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet("/invitations")]
    public async Task<IActionResult> Invitations(string? code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return View(new InvitationViewModel());
        }

        var invitation = await _db.Invitations.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Code == code && i.AcceptedAt == null && i.ExpiresAt > _clock.UtcNow, ct);

        if (invitation is null)
        {
            return View(new InvitationViewModel { Invalid = true });
        }

        return View(new InvitationViewModel { Email = invitation.Email, Code = invitation.Code, Role = invitation.Role });
    }

    [AllowAnonymous]
    [HttpPost("/invitations")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invitations(InvitationViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.Code == model.Code && i.AcceptedAt == null && i.ExpiresAt > _clock.UtcNow, ct);

        if (invitation is null)
        {
            model.Invalid = true;
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            ModelState.AddModelError(nameof(model.FullName), _t["Tell us your name."]);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = invitation.Email,
            Email = invitation.Email,
            EmailConfirmed = true,
            FullName = model.FullName.Trim(),
            JobTitle = string.IsNullOrWhiteSpace(model.JobTitle) ? null : model.JobTitle.Trim(),
            Role = invitation.Role,
            WorkspaceId = invitation.WorkspaceId,
            CapacityHoursPerWeek = 40m,
            CreatedAt = _clock.UtcNow
        };

        var created = await _users.CreateAsync(user, model.Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
            {
                ModelState.AddModelError(string.Empty, _t[error.Description]);
            }

            return View(model);
        }

        await _users.AddToRoleAsync(user, invitation.Role == WorkspaceRole.Admin ? "Admin" : "Member");
        invitation.AcceptedAt = _clock.UtcNow;
        invitation.AcceptedByUserId = user.Id;
        await _db.SaveChangesAsync(ct);

        await _signIn.SignInAsync(user, isPersistent: true);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("/culture")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult SetCulture(string culture, string? returnUrl)
    {
        var value = culture == "es" ? "es" : "en";
        // CookieRequestCultureProvider espera el formato "c=xx|uic=xx"; un código suelto se ignora.
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(value));
        Response.Cookies.Append("plexa.culture", cookieValue, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

        return Redirect(!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/dashboard");
    }
}

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Login", "Account");

    [AllowAnonymous]
    [Route("/error/{code:int}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error(int code)
    {
        Response.StatusCode = code;
        return View(code);
    }
}
