using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Infrastructure;

namespace Plexa.Web.Infrastructure;

public static class Policies
{
    public const string Authenticated = "Authenticated";
    public const string AdminOnly = "AdminOnly";
    public const string ProjectMember = "ProjectMember";
    public const string ProjectLeadOrAdmin = "ProjectLeadOrAdmin";
}

public enum ProjectAccessLevel
{
    Member,
    Lead
}

public sealed record ProjectRequirement(ProjectAccessLevel Level) : IAuthorizationRequirement;

/// <summary>Autorización por pertenencia al proyecto (regla 12): no miembro ⇒ 404.</summary>
public sealed class ProjectAccessHandler : AuthorizationHandler<ProjectRequirement>
{
    private readonly PlexaDbContext _db;
    private readonly IUserContext _user;

    public ProjectAccessHandler(PlexaDbContext db, IUserContext user)
    {
        _db = db;
        _user = user;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectRequirement requirement)
    {
        if (!_user.IsAuthenticated)
        {
            return;
        }

        var http = context.Resource as HttpContext;
        var raw = http?.Request.RouteValues.TryGetValue("id", out var id) == true
            ? id?.ToString()
            : http?.Request.Query["id"].FirstOrDefault();

        if (!Guid.TryParse(raw, out var projectId))
        {
            return;
        }

        var project = await _db.Projects.AsNoTracking()
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.WorkspaceId == _user.WorkspaceId);

        if (project is null)
        {
            return;
        }

        if (_user.IsAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (requirement.Level == ProjectAccessLevel.Lead)
        {
            if (project.LeadUserId == _user.UserId)
            {
                context.Succeed(requirement);
            }

            return;
        }

        if (project.LeadUserId == _user.UserId || project.Collaborators.Any(c => c.UserId == _user.UserId))
        {
            context.Succeed(requirement);
        }
    }
}
