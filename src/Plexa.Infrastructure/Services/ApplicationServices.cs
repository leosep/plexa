using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Entities;
using Plexa.Domain.Enums;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure.Services;

public sealed partial class MessageService : ServiceBase, IMessageService
{
    public MessageService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<IReadOnlyList<ThreadSummaryDto>> GetThreadsAsync(CancellationToken ct = default)
    {
        var threads = await Db.ChatThreads.AsNoTracking()
            .Include(t => t.Messages)
            .Include(t => t.Members)
            .Where(t => t.WorkspaceId == WorkspaceId)
            .Where(t => User.IsAdmin || t.Members.Any(m => m.UserId == User.UserId))
            .OrderByDescending(t => t.Messages.Max(m => (DateTime?)m.CreatedAt))
            .ToListAsync(ct);

        var projects = await Db.Projects.AsNoTracking()
            .Where(p => p.WorkspaceId == WorkspaceId)
            .ToDictionaryAsync(p => p.Id, p => $"{p.Code} · {p.Name}", ct);

        var users = await LoadUsersAsync(ct);

        return threads.Select(t =>
        {
            var last = t.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var membership = t.Members.FirstOrDefault(m => m.UserId == User.UserId);
            var unread = last is not null && (membership?.LastReadAt is null || last.CreatedAt > membership.LastReadAt);
            string? projectLabel = null;
            if (t.ProjectId.HasValue)
            {
                projects.TryGetValue(t.ProjectId.Value, out projectLabel);
            }

            return new ThreadSummaryDto(
                t.Id,
                t.Title,
                t.Scope,
                t.Scope == ThreadScope.Project ? "Project channel" : "Team channel",
                projectLabel,
                last?.Body,
                last is null ? null : PersonOf(users, last.AuthorId)?.FullName,
                last?.CreatedAt,
                unread);
        }).ToList();
    }

    public async Task<ThreadDto?> GetThreadAsync(Guid threadId, CancellationToken ct = default)
    {
        var thread = await Db.ChatThreads.AsNoTracking()
            .Include(t => t.Messages)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == threadId && t.WorkspaceId == WorkspaceId, ct);

        if (thread is null)
        {
            return null;
        }

        if (!User.IsAdmin && thread.Members.All(m => m.UserId != User.UserId))
        {
            return null;
        }

        var users = await LoadUsersAsync(ct);
        var project = thread.ProjectId.HasValue
            ? await Db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == thread.ProjectId.Value, ct)
            : null;

        var messages = thread.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(
                m.Id,
                m.Body,
                PersonOf(users, m.AuthorId) ?? PersonChip.From(m.AuthorId, "Member"),
                m.CreatedAt,
                m.CreatedAt.ToLocalTime().ToString("dd MMM HH:mm", System.Globalization.CultureInfo.GetCultureInfo("en-US")),
                m.AuthorId == User.UserId,
                string.IsNullOrWhiteSpace(m.Mentions) ? [] : m.Mentions.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ToList();

        return new ThreadDto(
            thread.Id,
            thread.Title,
            thread.Scope == ThreadScope.Project ? "Project channel" : "Team channel",
            project is null ? null : $"{project.Code} · {project.Name}",
            messages,
            true);
    }

    public async Task<(bool Ok, string? Error, Guid Id)> PostMessageAsync(Guid threadId, string body, Guid actorId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (false, "Write a message first.", Guid.Empty);
        }

        var thread = await Db.ChatThreads
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == threadId && t.WorkspaceId == WorkspaceId, ct);

        if (thread is null)
        {
            return (false, "Thread not found.", Guid.Empty);
        }

        if (!User.IsAdmin && thread.Members.All(m => m.UserId != User.UserId))
        {
            return (false, "Thread not found.", Guid.Empty);
        }

        var mentioned = new List<Guid>();
        var users = await LoadUsersAsync(ct);
        foreach (Match match in MentionRegex().Matches(body))
        {
            var handle = match.Groups[1].Value;
            var user = users.Values.FirstOrDefault(u =>
                string.Equals(u.UserName, handle, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.FullName.Replace(" ", string.Empty), handle.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase));
            if (user is not null)
            {
                mentioned.Add(user.Id);
            }
        }

        var message = new Message
        {
            ThreadId = threadId,
            AuthorId = actorId,
            Body = body.Trim(),
            Mentions = mentioned.Count == 0 ? null : string.Join(',', mentioned)
        };

        Db.Messages.Add(message);

        foreach (var userId in mentioned.Distinct())
        {
            Notify(userId, NotificationType.Mention, $"{User.FullName} mentioned you in \"{thread.Title}\"", actorId, threadId: threadId, projectId: thread.ProjectId);
        }

        var membership = thread.Members.FirstOrDefault(m => m.UserId == actorId);
        if (membership is not null)
        {
            membership.LastReadAt = Clock.UtcNow;
        }

        await Db.SaveChangesAsync(ct);
        return (true, null, message.Id);
    }

    public async Task<(bool Ok, string? Error, Guid Id)> CreateThreadAsync(string title, ThreadScope scope, Guid? projectId, Guid actorId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return (false, "The channel needs a title.", Guid.Empty);
        }

        var canCreate = User.IsAdmin;
        if (!canCreate && projectId.HasValue)
        {
            canCreate = await Db.Projects.AnyAsync(p => p.Id == projectId.Value && p.LeadUserId == User.UserId, ct);
        }

        if (!canCreate)
        {
            return (false, "Managers can open a project or team channel from a project page.", Guid.Empty);
        }

        var thread = new ChatThread
        {
            WorkspaceId = WorkspaceId,
            Title = title.Trim(),
            Scope = scope,
            ProjectId = projectId,
            CreatedByUserId = actorId
        };

        thread.Members.Add(new ThreadMember { UserId = actorId, LastReadAt = Clock.UtcNow });

        if (projectId.HasValue)
        {
            var project = await Db.Projects.AsNoTracking()
                .Include(p => p.Collaborators)
                .FirstOrDefaultAsync(p => p.Id == projectId.Value, ct);

            if (project?.LeadUserId is not null)
            {
                thread.Members.Add(new ThreadMember { UserId = project.LeadUserId.Value });
            }

            foreach (var collaborator in project?.Collaborators ?? [])
            {
                if (thread.Members.All(m => m.UserId != collaborator.UserId))
                {
                    thread.Members.Add(new ThreadMember { UserId = collaborator.UserId });
                }
            }
        }

        Db.ChatThreads.Add(thread);
        await Db.SaveChangesAsync(ct);
        return (true, null, thread.Id);
    }

    public async Task<int> UnreadCountAsync(CancellationToken ct = default)
    {
        var threads = await Db.ChatThreads.AsNoTracking()
            .Include(t => t.Messages)
            .Include(t => t.Members)
            .Where(t => t.WorkspaceId == WorkspaceId && t.Members.Any(m => m.UserId == User.UserId))
            .ToListAsync(ct);

        return threads.Count(t =>
        {
            var last = t.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var membership = t.Members.FirstOrDefault(m => m.UserId == User.UserId);
            return last is not null && (membership?.LastReadAt is null || last.CreatedAt > membership.LastReadAt);
        });
    }

    public async Task MarkReadAsync(Guid threadId, CancellationToken ct = default)
    {
        var membership = await Db.ThreadMembers.FirstOrDefaultAsync(m => m.ThreadId == threadId && m.UserId == User.UserId, ct);
        if (membership is null)
        {
            return;
        }

        membership.LastReadAt = Clock.UtcNow;
        await Db.SaveChangesAsync(ct);
    }

    [GeneratedRegex(@"@([\w\.\-]+(?:\s[\w\.\-]+)?)")]
    private static partial Regex MentionRegex();
}

public sealed class NotificationService : ServiceBase, INotificationService
{
    public NotificationService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int limit, CancellationToken ct = default)
    {
        var items = await Db.Notifications.AsNoTracking()
            .Where(n => n.UserId == User.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        var users = await LoadUsersAsync(ct);

        return items.Select(n => new NotificationDto(
            n.Id,
            n.Text,
            n.ActorId.HasValue && users.TryGetValue(n.ActorId.Value, out var actor) ? actor.FullName : null,
            n.CreatedAt,
            Relative(n.CreatedAt),
            n.IsRead,
            n.TaskId.HasValue ? $"/board?task={n.TaskId}" : n.ThreadId.HasValue ? $"/messages/{n.ThreadId}" : n.ProjectId.HasValue ? $"/projects/{n.ProjectId}" : null,
            n.Type)).ToList();
    }

    public Task<int> UnreadCountAsync(CancellationToken ct = default) =>
        Db.Notifications.CountAsync(n => n.UserId == User.UserId && n.ReadAt == null, ct);

    public async Task MarkAllReadAsync(CancellationToken ct = default)
    {
        var items = await Db.Notifications.Where(n => n.UserId == User.UserId && n.ReadAt == null).ToListAsync(ct);
        foreach (var item in items)
        {
            item.ReadAt = Clock.UtcNow;
        }

        await Db.SaveChangesAsync(ct);
    }

    private string Relative(DateTime utc)
    {
        var local = utc.ToLocalTime();
        var today = Clock.Today;
        var date = DateOnly.FromDateTime(local);
        if (date == today)
        {
            return "Today";
        }

        if (date == today.AddDays(-1))
        {
            return "Yesterday";
        }

        return local.ToString("dd MMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
    }
}

public sealed class SettingsService : ServiceBase, ISettingsService
{
    private static readonly string[] AvatarExtensions = [".png", ".jpg", ".jpeg", ".gif"];
    private const long MaxAvatarSize = 2L * 1024 * 1024;

    private readonly IFileStorage _storage;
    private readonly ISecretProtector _protector;

    public SettingsService(PlexaDbContext db, IUserContext user, IClock clock, IFileStorage storage, ISecretProtector protector)
        : base(db, user, clock)
    {
        _storage = storage;
        _protector = protector;
    }

    public async Task<ProfileDto> GetProfileAsync(CancellationToken ct = default)
    {
        var user = await Db.Users.AsNoTracking().FirstAsync(u => u.Id == User.UserId, ct);
        return new ProfileDto(
            user.ToPerson(),
            user.Email ?? string.Empty,
            user.Role == WorkspaceRole.Admin ? "Admin" : "Member",
            user.JobTitle,
            user.AvatarPath);
    }

    public async Task<(bool Ok, string? Error)> UpdateAvatarAsync(Stream content, string fileName, string contentType, long length, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AvatarExtensions.Contains(extension))
        {
            return (false, "Use a PNG, JPEG or GIF image.");
        }

        if (length > MaxAvatarSize)
        {
            return (false, "The image must be 2 MB or smaller.");
        }

        var user = await Db.Users.FirstAsync(u => u.Id == User.UserId, ct);
        var key = await _storage.SaveAsync(content, fileName, ct);
        var previous = user.AvatarPath;
        user.AvatarPath = $"/media/file/{Uri.EscapeDataString(key)}";
        await Db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(previous) && previous.StartsWith("/media/file/", StringComparison.Ordinal))
        {
            await _storage.DeleteAsync(Uri.UnescapeDataString(previous["/media/file/".Length..]), ct);
        }

        return (true, null);
    }

    public async Task<MailboxDto> GetMailboxAsync(CancellationToken ct = default)
    {
        var mailbox = await Db.UserMailboxes.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == User.UserId, ct);
        var user = await Db.Users.AsNoTracking().FirstAsync(u => u.Id == User.UserId, ct);
        return mailbox is null
            ? new MailboxDto(user.Email ?? string.Empty, string.Empty, 587, true, false)
            : new MailboxDto(mailbox.Email, mailbox.SmtpHost, mailbox.SmtpPort, mailbox.UseStartTls, !string.IsNullOrWhiteSpace(mailbox.ProtectedPassword));
    }

    public async Task<(bool Ok, string? Error)> SaveMailboxAsync(string email, string smtpHost, int smtpPort, bool useStartTls, string? password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return (false, "Enter a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            return (false, "Enter the SMTP server.");
        }

        var mailbox = await Db.UserMailboxes.FirstOrDefaultAsync(m => m.UserId == User.UserId, ct);
        if (mailbox is null)
        {
            mailbox = new UserMailbox { UserId = User.UserId };
            Db.UserMailboxes.Add(mailbox);
        }

        mailbox.Email = email.Trim();
        mailbox.SmtpHost = smtpHost.Trim();
        mailbox.SmtpPort = smtpPort <= 0 ? 587 : smtpPort;
        mailbox.UseStartTls = useStartTls;

        if (!string.IsNullOrWhiteSpace(password))
        {
            mailbox.ProtectedPassword = _protector.Protect(password);
        }

        await Db.SaveChangesAsync(ct);
        return (true, null);
    }
}

public sealed class AdminService : ServiceBase, IAdminService
{
    private readonly IMailer _mailer;
    private readonly IAppLinkGenerator _links;

    public AdminService(PlexaDbContext db, IUserContext user, IClock clock, IMailer mailer, IAppLinkGenerator links)
        : base(db, user, clock)
    {
        _mailer = mailer;
        _links = links;
    }

    public async Task<IReadOnlyList<SprintDto>> GetSprintsAsync(CancellationToken ct = default)
    {
        var sprints = await Db.Sprints.AsNoTracking()
            .Where(s => s.WorkspaceId == WorkspaceId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(ct);

        return sprints.Select(s => new SprintDto(
            s.Id, s.Name, s.StartDate, s.EndDate, s.CapacityPoints, s.CommittedPoints,
            s.Status, StatusLabels.Of(s.Status), s.Status == SprintStatus.Active)).ToList();
    }

    public async Task<(bool Ok, string? Error)> CreateSprintAsync(string name, DateOnly start, DateOnly end, int capacityPoints, int committedPoints, bool activate, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "The sprint needs a name.");
        }

        if (end < start)
        {
            return (false, "The end date must be after the start date.");
        }

        if (activate)
        {
            var active = await Db.Sprints.Where(s => s.WorkspaceId == WorkspaceId && s.Status == SprintStatus.Active).ToListAsync(ct);
            foreach (var sprint in active)
            {
                sprint.Status = SprintStatus.Closed;
            }
        }

        Db.Sprints.Add(new Sprint
        {
            WorkspaceId = WorkspaceId,
            Name = name.Trim(),
            StartDate = start,
            EndDate = end,
            CapacityPoints = Math.Max(0, capacityPoints),
            CommittedPoints = Math.Max(0, committedPoints),
            Status = activate ? SprintStatus.Active : SprintStatus.Planned
        });

        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> CloseSprintAsync(Guid sprintId, CancellationToken ct = default)
    {
        var sprint = await Db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId && s.WorkspaceId == WorkspaceId, ct);
        if (sprint is null)
        {
            return (false, "Sprint not found.");
        }

        sprint.Status = SprintStatus.Closed;
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<IReadOnlyList<AdminMemberDto>> GetMembersAsync(CancellationToken ct = default)
    {
        var users = await Db.Users.AsNoTracking()
            .Where(u => u.WorkspaceId == WorkspaceId)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        return users.Select(u => new AdminMemberDto(
            u.Id, u.FullName, u.Email ?? string.Empty, u.JobTitle, u.Role, u.CapacityHoursPerWeek, u.Initials)).ToList();
    }

    public async Task<(bool Ok, string? Error)> InviteMemberAsync(string email, WorkspaceRole role, string fullName, string jobTitle, decimal capacityHours, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return (false, "Enter a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (false, "Enter the person's name.");
        }

        if (await Db.Users.AnyAsync(u => u.Email == email, ct))
        {
            return (false, "That email already belongs to a member.");
        }

        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(6));
        Db.Invitations.Add(new Invitation
        {
            WorkspaceId = WorkspaceId,
            Email = email.Trim(),
            Code = code,
            Role = role,
            ExpiresAt = Clock.UtcNow.AddDays(14)
        });

        await Db.SaveChangesAsync(ct);

        var link = _links.AbsoluteUrl($"/invitations?code={code}&name={Uri.EscapeDataString(fullName)}&title={Uri.EscapeDataString(jobTitle)}&capacity={capacityHours}");
        await _mailer.SendAsync(email, "Your Plexa invitation",
            $"<p>Hello {fullName},</p><p>You have been invited to Plexa.</p><p>Invitation code: <strong>{code}</strong></p><p><a href=\"{link}\">Accept invitation</a></p>", ct);

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> UpdateMemberAsync(Guid userId, string fullName, string jobTitle, WorkspaceRole role, decimal capacityHours, CancellationToken ct = default)
    {
        var user = await Db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.WorkspaceId == WorkspaceId, ct);
        if (user is null)
        {
            return (false, "Member not found.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (false, "The name is required.");
        }

        user.FullName = fullName.Trim();
        user.JobTitle = string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle.Trim();
        user.Role = role;
        user.CapacityHoursPerWeek = Math.Clamp(capacityHours, 0m, 168m);
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<IReadOnlyList<AdminProjectDto>> GetProjectsAsync(CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var projects = await Db.Projects.AsNoTracking()
            .Where(p => p.WorkspaceId == WorkspaceId)
            .OrderBy(p => p.Code)
            .ToListAsync(ct);

        return projects.Select(p => new AdminProjectDto(
            p.Id, p.Code, p.Name, p.Client, p.Status, p.Health,
            PersonOf(users, p.LeadUserId)?.FullName ?? "No lead", p.EndDate)).ToList();
    }

    public async Task<(bool Ok, string? Error, Guid Id)> SaveProjectAsync(AdminProjectFormModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return (false, "The project needs a name.", Guid.Empty);
        }

        if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
        {
            return (false, "The end date must be after the start date.", Guid.Empty);
        }

        Project? project;
        if (model.Id.HasValue)
        {
            project = await Db.Projects.FirstOrDefaultAsync(p => p.Id == model.Id.Value && p.WorkspaceId == WorkspaceId, ct);
            if (project is null)
            {
                return (false, "Project not found.", Guid.Empty);
            }
        }
        else
        {
            project = new Project { WorkspaceId = WorkspaceId, Code = await NextProjectCodeAsync(ct) };
            Db.Projects.Add(project);
        }

        project.Name = model.Name.Trim();
        project.Client = string.IsNullOrWhiteSpace(model.Client) ? "Internal development" : model.Client.Trim();
        project.LeadUserId = model.LeadUserId;
        project.DepartmentId = model.DepartmentId;
        project.Status = model.Status;
        project.Health = model.Health;
        project.StartDate = model.StartDate;
        project.EndDate = model.EndDate;
        project.Budget = model.Budget;

        await Db.SaveChangesAsync(ct);
        return (true, null, project.Id);
    }

    public async Task<(bool Ok, string? Error)> ArchiveProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await Db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.WorkspaceId == WorkspaceId, ct);
        if (project is null)
        {
            return (false, "Project not found.");
        }

        project.Status = ProjectStatus.Archived;
        project.ArchivedAt = Clock.UtcNow;
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }
}

public sealed class SearchService : ServiceBase, ISearchService
{
    private readonly ITaskService _tasks;

    public SearchService(PlexaDbContext db, IUserContext user, IClock clock, ITaskService tasks) : base(db, user, clock)
    {
        _tasks = tasks;
    }

    public Task<IReadOnlyList<SearchHitDto>> SuggestAsync(string query, CancellationToken ct = default) =>
        _tasks.SearchAsync(query, ct);
}
