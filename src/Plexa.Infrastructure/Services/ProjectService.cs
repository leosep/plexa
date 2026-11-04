using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Entities;
using Plexa.Domain.Enums;
using Plexa.Domain.Rules;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure.Services;

public sealed class ProjectService : ServiceBase, IProjectService
{
    private static readonly string[] AllowedExtensions =
    [
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".ods",
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".txt", ".md", ".csv", ".zip"
    ];

    private const long MaxFileSize = 25L * 1024 * 1024;

    private readonly ITaskService _tasks;
    private readonly ICalendarService _calendar;
    private readonly IFileStorage _storage;

    public ProjectService(PlexaDbContext db, IUserContext user, IClock clock, ITaskService tasks, ICalendarService calendar, IFileStorage storage)
        : base(db, user, clock)
    {
        _tasks = tasks;
        _calendar = calendar;
        _storage = storage;
    }

    public async Task<ProjectListDto> ListAsync(ProjectStatus? status, string? query, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var projects = await VisibleProjects()
            .Include(p => p.Tasks)
            .Include(p => p.Collaborators)
            .OrderBy(p => p.Code)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            projects = projects.Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                           p.Client.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                           p.Code.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var all = projects.Select(p => ToListItem(p, users)).ToList();

        var counts = new Dictionary<ProjectStatus, int>
        {
            [ProjectStatus.InProgress] = all.Count(p => p.Status == ProjectStatus.InProgress),
            [ProjectStatus.AtRisk] = all.Count(p => p.Status == ProjectStatus.AtRisk || p.Health == ProjectHealth.AtRisk),
            [ProjectStatus.Planning] = all.Count(p => p.Status == ProjectStatus.Planning),
            [ProjectStatus.OnHold] = all.Count(p => p.Status == ProjectStatus.OnHold),
            [ProjectStatus.Completed] = all.Count(p => p.Status == ProjectStatus.Completed),
            [ProjectStatus.Archived] = all.Count(p => p.Status == ProjectStatus.Archived)
        };

        var filtered = status switch
        {
            null => all,
            ProjectStatus.AtRisk => all.Where(p => p.Status == ProjectStatus.AtRisk || p.Health == ProjectHealth.AtRisk).ToList(),
            _ => all.Where(p => p.Status == status).ToList()
        };

        var active = all.Where(p => p.Status is not (ProjectStatus.Completed or ProjectStatus.Archived)).ToList();
        var budget = active.Sum(p => p.Budget ?? 0m);
        var averageProgress = active.Count == 0 ? 0 : (int)Math.Round(active.Average(p => p.ProgressPct));

        var kpis = new List<KpiCard>
        {
            KpiCard.Neutral("Active projects", active.Count.ToString()),
            KpiCard.Danger("At risk", all.Count(p => p.Health == ProjectHealth.AtRisk).ToString()),
            KpiCard.Neutral("Budget in progress", $"US${budget:0}"),
            KpiCard.Neutral("Average progress", $"{averageProgress}%")
        };

        var chips = new List<StatusFilterChip>
        {
            new(null, "All", all.Count, status is null),
            new(ProjectStatus.InProgress, "In progress", counts[ProjectStatus.InProgress], status == ProjectStatus.InProgress),
            new(ProjectStatus.AtRisk, "At risk", counts[ProjectStatus.AtRisk], status == ProjectStatus.AtRisk),
            new(ProjectStatus.Planning, "Planning", counts[ProjectStatus.Planning], status == ProjectStatus.Planning),
            new(ProjectStatus.OnHold, "On hold", counts[ProjectStatus.OnHold], status == ProjectStatus.OnHold),
            new(ProjectStatus.Completed, "Completed", counts[ProjectStatus.Completed], status == ProjectStatus.Completed),
            new(ProjectStatus.Archived, "Archived", counts[ProjectStatus.Archived], status == ProjectStatus.Archived)
        };

        return new ProjectListDto(kpis, chips, filtered, filtered.Count, status, query);
    }

    public async Task<ProjectDetailDto?> GetDetailAsync(Guid projectId, string tab, CancellationToken ct = default)
    {
        var project = await Db.Projects.AsNoTracking()
            .Include(p => p.Tasks)
            .Include(p => p.Collaborators)
            .Include(p => p.Phases)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.WorkspaceId == WorkspaceId, ct);

        if (project is null || !await CanAccessAsync(projectId, ct))
        {
            return null;
        }

        var users = await LoadUsersAsync(ct);
        var activeTab = NormalizeTab(tab);

        var header = new ProjectHeaderDto(
            project.Id,
            project.Code,
            project.Name,
            project.Client,
            InitialsOf(project.Name),
            project.Health,
            StatusLabels.Of(project.Health),
            PersonOf(users, project.LeadUserId),
            ProgressOf(project),
            project.StartDate,
            project.EndDate,
            project.Tasks.Count(t => t.Status == WorkTaskStatus.Completed),
            project.Tasks.Count,
            MetaLine(project),
            User.IsAdmin || project.LeadUserId == User.UserId);

        ProjectOverviewDto? overview = null;
        ProjectDashboardDto? dashboard = null;
        BoardDto? board = null;
        ProjectTimelineDto? timeline = null;
        ProjectCalendarDto? calendar = null;
        ProjectTeamDto? team = null;
        ProjectDocumentsDto? documents = null;

        var activity = await LoadActivityAsync(project.Id, users, ct);
        var phases = project.Phases.OrderBy(p => p.Position).Select(p => new PhaseDto(p.Id, p.Name, p.StartDate, p.EndDate, p.Position)).ToList();

        switch (activeTab)
        {
            case "Overview":
                board = await _tasks.GetBoardAsync(new BoardQuery(ProjectId: project.Id), ct);
                overview = new ProjectOverviewDto(phases, board.Columns, activity, true, RangeLabel(project));
                break;

            case "Dashboard":
                var done = project.Tasks.Count(t => t.Status == WorkTaskStatus.Completed);
                var total = project.Tasks.Count;
                var points = project.Tasks.Where(t => t.Status == WorkTaskStatus.Completed).Sum(t => t.StoryPoints);
                var totalPoints = project.Tasks.Sum(t => t.StoryPoints);
                var openTasks = project.Tasks.Where(t => t.Status != WorkTaskStatus.Completed).ToList();
                var overdue = project.Tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < Clock.Today && t.Status != WorkTaskStatus.Completed);

                dashboard = new ProjectDashboardDto(
                    [
                        KpiCard.Success("Tasks completed", $"{done} / {total}", total == 0 ? "0% of the project" : $"{Math.Round(done * 100d / total)}% of the project"),
                        KpiCard.Neutral("Points delivered", $"{points} / {totalPoints}"),
                        KpiCard.Neutral("Estimated effort", DurationFormatter.Format(openTasks.Sum(t => t.EstimateHours)), $"{openTasks.Count} open tasks"),
                        overdue > 0 ? KpiCard.Danger("Overdue", overdue.ToString(), "Review today") : KpiCard.Success("Overdue", "0", "On track")
                    ],
                    project.Milestones.OrderBy(m => m.DueDate).Select(m => new MilestoneDto(m.Id, m.Title, m.DueDate, m.Done)).ToList(),
                    activity);
                break;

            case "Board":
                board = await _tasks.GetBoardAsync(new BoardQuery(ProjectId: project.Id), ct);
                break;

            case "Timeline":
                timeline = new ProjectTimelineDto(RangeLabel(project), phases);
                break;

            case "Calendar":
                calendar = new ProjectCalendarDto(await _calendar.GetForProjectAsync(project.Id, ct));
                break;

            case "Team":
                var department = project.DepartmentId.HasValue
                    ? await Db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == project.DepartmentId.Value, ct)
                    : null;
                team = new ProjectTeamDto(
                    department?.Name,
                    PersonOf(users, department?.LeadUserId),
                    PersonOf(users, project.LeadUserId),
                    project.Collaborators.Select(c => PersonOf(users, c.UserId)).Where(p => p is not null).Select(p => p!).ToList(),
                    User.IsAdmin || project.LeadUserId == User.UserId);
                break;

            case "Documents":
                documents = new ProjectDocumentsDto(project.Documents
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => new ProjectDocumentDto(
                        d.Id,
                        d.FileName,
                        d.ContentType,
                        d.SizeBytes,
                        PersonOf(users, d.UploadedByUserId),
                        d.CreatedAt,
                        FormatSize(d.SizeBytes)))
                    .ToList());
                break;
        }

        return new ProjectDetailDto(header, activeTab, overview, dashboard, board, timeline, calendar, team, documents);
    }

    public async Task<TeamEditOptionsDto?> GetTeamEditAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await Db.Projects.AsNoTracking()
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.WorkspaceId == WorkspaceId, ct);

        if (project is null || !(User.IsAdmin || project.LeadUserId == User.UserId))
        {
            return null;
        }

        var departments = await Db.Departments.AsNoTracking()
            .Where(d => d.WorkspaceId == WorkspaceId)
            .OrderBy(d => d.Name)
            .Select(d => new ValueTuple<Guid, string>(d.Id, d.Name))
            .ToListAsync(ct);

        var users = await LoadUsersAsync(ct);
        var model = new TeamEditModel
        {
            ProjectId = project.Id,
            DepartmentId = project.DepartmentId,
            LeadUserId = project.LeadUserId,
            CollaboratorIds = project.Collaborators.Select(c => c.UserId).ToList()
        };

        return new TeamEditOptionsDto(
            departments.Select(d => (d.Item1, d.Item2)).ToList(),
            users.Values.OrderBy(u => u.FullName).Select(u => u.ToPerson()).ToList(),
            model);
    }

    public async Task<(bool Ok, string? Error)> SaveTeamAsync(TeamEditModel model, CancellationToken ct = default)
    {
        var project = await Db.Projects
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.Id == model.ProjectId && p.WorkspaceId == WorkspaceId, ct);

        if (project is null)
        {
            return (false, "Project not found.");
        }

        if (!(User.IsAdmin || project.LeadUserId == User.UserId))
        {
            return (false, "You do not have permission to edit this team.");
        }

        project.DepartmentId = model.DepartmentId;
        project.LeadUserId = model.LeadUserId;

        Db.ProjectCollaborators.RemoveRange(project.Collaborators);
        foreach (var userId in model.CollaboratorIds.Distinct())
        {
            Db.ProjectCollaborators.Add(new ProjectCollaborator { ProjectId = project.Id, UserId = userId });
        }

        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> UploadDocumentsAsync(
        Guid projectId,
        IReadOnlyList<(string FileName, string ContentType, long Length, Stream Content)> files,
        Guid actorId,
        CancellationToken ct = default)
    {
        if (!await CanAccessAsync(projectId, ct))
        {
            return (false, "Project not found.");
        }

        if (files.Count == 0)
        {
            return (false, "Choose at least one file.");
        }

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return (false, $"“{file.FileName}” is not an allowed file type.");
            }

            if (file.Length > MaxFileSize)
            {
                return (false, $"“{file.FileName}” is larger than 25 MB.");
            }
        }

        foreach (var file in files)
        {
            var key = await _storage.SaveAsync(file.Content, file.FileName, ct);
            Db.Documents.Add(new Document
            {
                WorkspaceId = WorkspaceId,
                ProjectId = projectId,
                FileName = Path.GetFileName(file.FileName),
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                SizeBytes = file.Length,
                UploadedByUserId = actorId,
                StorageKey = key
            });
            AddActivity(projectId, actorId, ActivityVerb.DocumentUploaded, null, $"Document \"{Path.GetFileName(file.FileName)}\" uploaded");
        }

        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> DeleteDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        var document = await Db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.WorkspaceId == WorkspaceId, ct);
        if (document is null)
        {
            return (false, "Document not found.");
        }

        if (!await CanAccessAsync(document.ProjectId, ct))
        {
            return (false, "Document not found.");
        }

        await _storage.DeleteAsync(document.StorageKey, ct);
        Db.Documents.Remove(document);
        AddActivity(document.ProjectId, User.UserId, ActivityVerb.Created, null, $"Document \"{document.FileName}\" deleted");
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(string FileName, string ContentType, Stream Content)?> OpenDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        var document = await Db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.WorkspaceId == WorkspaceId, ct);
        if (document is null || !await CanAccessAsync(document.ProjectId, ct))
        {
            return null;
        }

        var stream = await _storage.OpenReadAsync(document.StorageKey, ct);
        return stream is null ? null : (document.FileName, document.ContentType, stream);
    }

    public async Task<IReadOnlyList<ProjectOption>> GetVisibleProjectsAsync(CancellationToken ct = default) =>
        await VisibleProjects().OrderBy(p => p.Code)
            .Select(p => new ProjectOption(p.Id, p.Code, p.Name, p.Code + " · " + p.Name))
            .ToListAsync(ct);

    public async Task<bool> CanAccessAsync(Guid projectId, CancellationToken ct = default)
    {
        if (User.IsAdmin)
        {
            return await Db.Projects.AnyAsync(p => p.Id == projectId && p.WorkspaceId == WorkspaceId, ct);
        }

        return await Db.Projects.AsNoTracking()
            .AnyAsync(p => p.Id == projectId && p.WorkspaceId == WorkspaceId &&
                (p.LeadUserId == User.UserId || p.Collaborators.Any(c => c.UserId == User.UserId)), ct);
    }

    private ProjectListItemDto ToListItem(Project project, Dictionary<Guid, ApplicationUser> users) =>
        new(
            project.Id,
            project.Code,
            project.Name,
            project.Client,
            InitialsOf(project.Name),
            PersonOf(users, project.LeadUserId),
            project.Status,
            StatusLabels.Of(project.Status),
            project.Health,
            ProgressOf(project),
            project.Budget,
            project.EndDate,
            project.Tasks.Count(t => t.Status == WorkTaskStatus.Completed),
            project.Tasks.Count);

    private static int ProgressOf(Project project) =>
        ProjectProgressCalculator.Calculate(project.Tasks.Select(t => (t.EstimateHours, t.Status == WorkTaskStatus.Completed)));

    private static string InitialsOf(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => string.Concat(parts[0][..1], parts[1][..1]).ToUpperInvariant()
        };
    }

    private static string MetaLine(Project project)
    {
        var range = project.StartDate.HasValue && project.EndDate.HasValue
            ? $"{project.StartDate.Value:dd MMM} → {project.EndDate.Value:dd MMM}"
            : "No dates";
        return $"{project.Client.ToUpperInvariant()} · {project.Code} · {range}";
    }

    private static string RangeLabel(Project project) =>
        project.StartDate.HasValue && project.EndDate.HasValue
            ? $"{project.StartDate.Value:dd MMM} → {project.EndDate.Value:dd MMM}"
            : "No dates defined";

    private static string NormalizeTab(string tab) => tab switch
    {
        "Dashboard" or "Board" or "Timeline" or "Calendar" or "Team" or "Documents" => tab,
        _ => "Overview"
    };

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / 1024d / 1024d:0.#} MB",
        >= 1024 => $"{bytes / 1024d:0.#} KB",
        _ => $"{bytes} B"
    };

    private async Task<IReadOnlyList<ActivityItemDto>> LoadActivityAsync(Guid projectId, Dictionary<Guid, ApplicationUser> users, CancellationToken ct)
    {
        var logs = await Db.ActivityLogs.AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(30)
            .ToListAsync(ct);

        return logs.Select(a => new ActivityItemDto(
            a.Id,
            a.Text,
            PersonOf(users, a.ActorId) ?? PersonChip.From(a.ActorId, "System"),
            a.CreatedAt,
            FormatDate(a.CreatedAt),
            a.Verb switch
            {
                ActivityVerb.Created => ActivityVerbKind.Created,
                ActivityVerb.Moved => ActivityVerbKind.Moved,
                ActivityVerb.MarkedDone => ActivityVerbKind.Completed,
                ActivityVerb.Reopened => ActivityVerbKind.Reopened,
                _ => ActivityVerbKind.Other
            })).ToList();
    }

    private static string FormatDate(DateTime utc) => utc.ToLocalTime().ToString("dd MMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
}
