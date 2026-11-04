using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Domain.Entities;
using Plexa.Domain.Enums;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}

public static class Mappers
{
    public static PersonChip ToPerson(this ApplicationUser user) =>
        PersonChip.From(user.Id, user.FullName, user.JobTitle, user.AvatarPath);

    public static string StatusLabel(this WorkTaskStatus status) => Application.Dtos.StatusLabels.Of(status);
}

public abstract class ServiceBase
{
    protected readonly PlexaDbContext Db;
    protected readonly IUserContext User;
    protected readonly IClock Clock;

    protected ServiceBase(PlexaDbContext db, IUserContext user, IClock clock)
    {
        Db = db;
        User = user;
        Clock = clock;
    }

    protected Guid WorkspaceId => User.WorkspaceId;

    /// <summary>Proyectos visibles para el usuario actual (regla 12).</summary>
    protected IQueryable<Project> VisibleProjects()
    {
        var query = Db.Projects.AsNoTracking().Where(p => p.WorkspaceId == WorkspaceId);
        if (User.IsAdmin)
        {
            return query;
        }

        var userId = User.UserId;
        return query.Where(p => p.LeadUserId == userId || p.Collaborators.Any(c => c.UserId == userId));
    }

    protected async Task<Dictionary<Guid, ApplicationUser>> LoadUsersAsync(CancellationToken ct)
    {
        return await Db.Users.AsNoTracking()
            .Where(u => u.WorkspaceId == WorkspaceId)
            .ToDictionaryAsync(u => u.Id, ct);
    }

    protected PersonChip? PersonOf(Dictionary<Guid, ApplicationUser> users, Guid? userId) =>
        userId.HasValue && users.TryGetValue(userId.Value, out var user) ? user.ToPerson() : null;

    protected void AddActivity(Guid? projectId, Guid actorId, Domain.Enums.ActivityVerb verb, Guid? taskId, string text)
    {
        Db.ActivityLogs.Add(new ActivityLog
        {
            WorkspaceId = WorkspaceId,
            ProjectId = projectId,
            ActorId = actorId,
            Verb = verb,
            TaskId = taskId,
            Text = text
        });
    }

    protected void Notify(Guid userId, Domain.Enums.NotificationType type, string text, Guid? actorId, Guid? taskId = null, Guid? projectId = null, Guid? threadId = null)
    {
        if (userId == User.UserId)
        {
            return;
        }

        Db.Notifications.Add(new Notification
        {
            WorkspaceId = WorkspaceId,
            UserId = userId,
            Type = type,
            Text = text,
            ActorId = actorId,
            TaskId = taskId,
            ProjectId = projectId,
            ThreadId = threadId
        });
    }

    protected async Task<string> NextProjectCodeAsync(CancellationToken ct)
    {
        var counter = await Db.WorkspaceCounters.FirstOrDefaultAsync(c => c.WorkspaceId == WorkspaceId && c.Key == "project", ct);
        if (counter is null)
        {
            counter = new WorkspaceCounter { WorkspaceId = WorkspaceId, Key = "project", Value = 0 };
            Db.WorkspaceCounters.Add(counter);
        }

        counter.Value += 1;
        await Db.SaveChangesAsync(ct);
        return $"PRY-{counter.Value:00}";
    }
}
