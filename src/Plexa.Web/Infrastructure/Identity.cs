using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Plexa.Application.Abstractions;
using Plexa.Domain.Enums;
using Plexa.Infrastructure.Identity;

namespace Plexa.Web.Infrastructure;

public static class PlexaClaims
{
    public const string WorkspaceId = "plexa:workspace";
    public const string Role = "plexa:role";
    public const string FullName = "plexa:name";
    public const string JobTitle = "plexa:job";
    public const string Avatar = "plexa:avatar";
}

public sealed class PlexaClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>
{
    public PlexaClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(PlexaClaims.WorkspaceId, user.WorkspaceId.ToString()));
        identity.AddClaim(new Claim(PlexaClaims.Role, user.Role == WorkspaceRole.Admin ? "Admin" : "Member"));
        identity.AddClaim(new Claim(PlexaClaims.FullName, user.FullName));
        if (!string.IsNullOrWhiteSpace(user.JobTitle))
        {
            identity.AddClaim(new Claim(PlexaClaims.JobTitle, user.JobTitle));
        }

        if (!string.IsNullOrWhiteSpace(user.AvatarPath))
        {
            identity.AddClaim(new Claim(PlexaClaims.Avatar, user.AvatarPath));
        }

        return identity;
    }
}

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpUserContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public Guid UserId => Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    public Guid WorkspaceId => Guid.TryParse(Principal?.FindFirstValue(PlexaClaims.WorkspaceId), out var id) ? id : Guid.Empty;

    public string FullName => Principal?.FindFirstValue(PlexaClaims.FullName) ?? Principal?.Identity?.Name ?? "Guest";

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public string? JobTitle => Principal?.FindFirstValue(PlexaClaims.JobTitle);

    public string? AvatarPath => Principal?.FindFirstValue(PlexaClaims.Avatar);

    public WorkspaceRole Role => Principal?.FindFirstValue(PlexaClaims.Role) == "Admin" ? WorkspaceRole.Admin : WorkspaceRole.Member;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsAdmin => Role == WorkspaceRole.Admin;
}

public sealed class AppLinkGenerator : IAppLinkGenerator
{
    private readonly IHttpContextAccessor _accessor;

    public AppLinkGenerator(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string AbsoluteUrl(string relativePath)
    {
        var request = _accessor.HttpContext?.Request;
        if (request is null)
        {
            return relativePath;
        }

        return $"{request.Scheme}://{request.Host}{relativePath}";
    }
}
