using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Entities;
using Plexa.Domain.Enums;
using Plexa.Domain.Rules;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure.Services;

public sealed class TaskService : ServiceBase, ITaskService
{
    private readonly ITranslator _t;

    public TaskService(PlexaDbContext db, IUserContext user, IClock clock, ITranslator translator) : base(db, user, clock)
    {
        _t = translator;
    }

    public async Task<BoardDto> GetBoardAsync(BoardQuery query, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var activeTimer = await Db.TimeEntries.AsNoTracking()
            .Where(t => t.UserId == User.UserId && t.EndedAt == null)
            .Select(t => (Guid?)t.TaskId)
            .FirstOrDefaultAsync(ct);

        var q = Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId);

        if (!User.IsAdmin)
        {
            var visible = VisibleProjects().Select(p => p.Id);
            q = q.Where(t => t.ProjectId == null || visible.Contains(t.ProjectId.Value));
        }

        if (query.ProjectId.HasValue)
        {
            q = q.Where(t => t.ProjectId == query.ProjectId.Value);
        }
        else if (query.LooseOnly)
        {
            q = q.Where(t => t.ProjectId == null);
        }

        if (query.AssigneeId.HasValue)
        {
            q = q.Where(t => t.AssigneeId == query.AssigneeId.Value ||
                t.Subtasks.Any(s => s.AssigneeId == query.AssigneeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            q = q.Where(t => t.Title.Contains(term) || (t.Description != null && t.Description.Contains(term)) ||
                (t.Parent != null && t.Parent.Title.Contains(term)));
        }

        var tasks = await q
            .Include(t => t.Project)
            .Include(t => t.Parent)
            .Include(t => t.Subtasks).ThenInclude(s => s.TimeEntries)
            .Include(t => t.TimeEntries)
            .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
            .Include(t => t.Checklist)
            .OrderBy(t => t.Priority == TaskPriority.High ? 0 : t.Priority == TaskPriority.Medium ? 1 : 2)
            .ThenBy(t => t.DueDate)
            .ToListAsync(ct);

        var searching = !string.IsNullOrWhiteSpace(query.Q);
        var cards = tasks
            .Where(t => searching || t.ParentId == null)
            .Select(t => BuildCard(t, users, activeTimer))
            .ToList();

        var columns = new List<BoardColumnDto>();
        foreach (var status in new[] { WorkTaskStatus.ToDo, WorkTaskStatus.InProgress, WorkTaskStatus.Review, WorkTaskStatus.Completed })
        {
            var items = cards.Where(c => c.Status == status).ToList();
            columns.Add(new BoardColumnDto(status, StatusLabels.Of(status), items.Count, items));
        }

        var projects = await VisibleProjects()
            .OrderBy(p => p.Code)
            .Select(p => new ProjectOption(p.Id, p.Code, p.Name, p.Code + " · " + p.Name))
            .ToListAsync(ct);

        var people = users.Values.OrderBy(u => u.FullName).Select(u => u.ToPerson()).ToList();

        var projectLabel = query.ProjectId.HasValue
            ? projects.FirstOrDefault(p => p.Id == query.ProjectId.Value)?.Label ?? _t["Project"]
            : query.LooseOnly ? _t["Loose tasks"] : _t["All projects"];

        var subtitle = _t.Format("{0} · Backlog · {1} tasks", projectLabel, tasks.Count);
        if (searching)
        {
            subtitle += " " + _t.Format("· matching “{0}”", query.Q!.Trim());
        }

        var hasActiveSprint = await Db.Sprints.AnyAsync(s => s.WorkspaceId == WorkspaceId && s.Status == SprintStatus.Active, ct);

        return new BoardDto(
            columns,
            new BoardFiltersDto(query.ProjectId, query.LooseOnly, query.AssigneeId, query.Q, projects, people, hasActiveSprint),
            tasks.Count,
            subtitle,
            query.OpenTaskId);
    }

    public async Task<TaskCardDto?> GetCardAsync(Guid taskId, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var activeTimer = await Db.TimeEntries.AsNoTracking()
            .Where(t => t.UserId == User.UserId && t.EndedAt == null)
            .Select(t => (Guid?)t.TaskId)
            .FirstOrDefaultAsync(ct);

        var task = await LoadTaskGraph().FirstOrDefaultAsync(t => t.Id == taskId, ct);
        return task is null ? null : BuildCard(task, users, activeTimer);
    }

    public async Task<TaskDetailDto?> GetDetailAsync(Guid taskId, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var task = await LoadTaskGraph().FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (task is null)
        {
            return null;
        }

        var activeTimer = await Db.TimeEntries.AsNoTracking()
            .Where(t => t.UserId == User.UserId && t.EndedAt == null)
            .FirstOrDefaultAsync(ct);

        var weights = task.Subtasks.Select(s => new SubtaskWeight(s.EstimateHours, s.Status == WorkTaskStatus.Completed)).ToList();
        var blockers = BlockingRule.PendingBlockers(task.Dependencies
            .Select(d => new Blocker(d.DependsOnTask?.Title ?? "?", d.DependsOnTask?.Status ?? WorkTaskStatus.ToDo, d.DependsOnTask?.ApprovedAt)));
        var logged = TimeSpan.FromSeconds(task.TimeEntries.Sum(e => e.Seconds) + task.Subtasks.Sum(s => s.TimeEntries.Sum(e => e.Seconds)));

        var options = await BuildDependencyOptionsAsync(task.Id, task.Subtasks.Select(s => s.Id).ToList(), ct);
        var canApprove = await CanApproveAsync(task.ProjectId, ct);

        return new TaskDetailDto(
            task.Id,
            task.Title,
            task.Description,
            task.ProjectId,
            task.Project?.Code,
            task.Project?.Name,
            task.ParentId,
            task.Parent?.Title,
            task.Status,
            StatusLabels.Of(task.Status),
            task.Priority,
            StatusLabels.Of(task.Priority),
            task.Tag,
            PersonOf(users, task.AssigneeId),
            BuildAssignedTeam(task, users),
            task.StartDate,
            task.DueDate,
            task.EstimateHours,
            task.StoryPoints,
            task.ProgressMode,
            logged,
            ProgressCalculator.Calculate(task.ProgressMode, weights, task.Status == WorkTaskStatus.Completed),
            task.Checklist.Count(c => c.Done),
            task.Checklist.Count,
            task.Checklist.OrderBy(c => c.Position).Select(c => new ChecklistItemDto(c.Id, c.Text, c.Done)).ToList(),
            BuildSubtaskLines(task, users),
            ProgressCalculator.UnallocatedHours(task.EstimateHours, weights),
            blockers.Count > 0,
            blockers.Select(b => $"{b.TaskTitle} · {_t[BlockingRule.StatusLabel(b.Status)]}").ToList(),
            blockers.Count > 0 ? _t.Format("It cannot start yet; it waits for: {0}", DescribeBlockers(blockers)) : null,
            activeTimer?.TaskId == task.Id,
            activeTimer?.TaskId == task.Id ? activeTimer.StartedAt : null,
            task.Status == WorkTaskStatus.Completed && task.ApprovedAt is null,
            !task.IsSubtask && task.Status == WorkTaskStatus.ToDo && task.TimeEntries.Count == 0,
            task.Subtasks.Count == 0,
            options,
            task.CreatedAt,
            task.IsSubtask);
    }

    public async Task<TaskFormOptionsDto> GetFormOptionsAsync(Guid? taskId = null, Guid? projectId = null, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var projects = await VisibleProjects().OrderBy(p => p.Code)
            .Select(p => new ProjectOption(p.Id, p.Code, p.Name, p.Code + " · " + p.Name))
            .ToListAsync(ct);

        var exclude = new List<Guid>();
        if (taskId.HasValue)
        {
            exclude.Add(taskId.Value);
            exclude.AddRange(await Db.Tasks.Where(t => t.ParentId == taskId.Value).Select(t => t.Id).ToListAsync(ct));
        }

        var options = await BuildDependencyOptionsAsync(taskId, exclude, ct);
        var defaults = new TaskFormModel { ProjectId = projectId };

        return new TaskFormOptionsDto(projects, users.Values.OrderBy(u => u.FullName).Select(u => u.ToPerson()).ToList(), options, defaults);
    }

    public async Task<TaskFormModel?> GetFormAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await Db.Tasks.AsNoTracking()
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (task is null)
        {
            return null;
        }

        return new TaskFormModel
        {
            Id = task.Id,
            Title = task.Title,
            ProjectId = task.ProjectId,
            ParentId = task.ParentId,
            AssigneeId = task.AssigneeId,
            Priority = task.Priority,
            Tag = task.Tag,
            DueDate = task.DueDate,
            StartDate = task.StartDate,
            EstimateHours = task.EstimateHours,
            StoryPoints = task.StoryPoints,
            InBacklog = task.InBacklog,
            ProgressMode = task.ProgressMode,
            Description = task.Description,
            DependsOn = task.Dependencies.Select(d => d.DependsOnTaskId).ToList()
        };
    }

    public async Task<(bool Ok, string? Error, Guid TaskId)> CreateAsync(TaskFormModel model, Guid actorId, CancellationToken ct = default)
    {
        var validation = await ValidateFormAsync(model, null, ct);
        if (validation is not null)
        {
            return (false, validation, Guid.Empty);
        }

        WorkTask? parent = null;
        if (model.ParentId.HasValue)
        {
            parent = await Db.Tasks.FirstOrDefaultAsync(t => t.Id == model.ParentId.Value, ct);
            if (parent is null)
            {
                return (false, "Parent task not found.", Guid.Empty);
            }

            if (parent.ParentId.HasValue)
            {
                return (false, "A subtask cannot have subtasks.", Guid.Empty);
            }
        }

        var task = new WorkTask
        {
            WorkspaceId = WorkspaceId,
            ProjectId = parent?.ProjectId ?? model.ProjectId,
            ParentId = model.ParentId,
            Title = model.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            AssigneeId = model.AssigneeId,
            Priority = model.Priority,
            Tag = string.IsNullOrWhiteSpace(model.Tag) ? null : model.Tag,
            StartDate = model.StartDate,
            DueDate = model.DueDate,
            EstimateHours = model.EstimateHours,
            StoryPoints = model.StoryPoints,
            ProgressMode = model.ProgressMode,
            InBacklog = model.InBacklog,
            Status = WorkTaskStatus.ToDo
        };

        Db.Tasks.Add(task);

        foreach (var dependencyId in model.DependsOn.Distinct())
        {
            Db.TaskDependencies.Add(new TaskDependency { TaskId = task.Id, DependsOnTaskId = dependencyId });
        }

        foreach (var subtask in model.Subtasks.Where(s => !string.IsNullOrWhiteSpace(s.Title)))
        {
            Db.Tasks.Add(new WorkTask
            {
                WorkspaceId = WorkspaceId,
                ProjectId = task.ProjectId,
                ParentId = task.Id,
                Title = subtask.Title.Trim(),
                AssigneeId = subtask.AssigneeId,
                EstimateHours = subtask.Hours,
                DueDate = subtask.DueDate,
                Status = WorkTaskStatus.ToDo,
                InBacklog = task.InBacklog,
                Priority = TaskPriority.Medium,
                ProgressMode = TaskProgressMode.Hours
            });
        }

        AddActivity(task.ProjectId, actorId, ActivityVerb.Created, task.Id, $"Task \"{task.Title}\" created");
        if (task.AssigneeId.HasValue)
        {
            Notify(task.AssigneeId.Value, NotificationType.TaskAssigned, $"Task \"{task.Title}\" assigned to you", actorId, task.Id, task.ProjectId);
        }

        await Db.SaveChangesAsync(ct);
        return (true, null, task.Id);
    }

    public async Task<(bool Ok, string? Error)> UpdateAsync(TaskFormModel model, Guid actorId, CancellationToken ct = default)
    {
        if (!model.Id.HasValue)
        {
            return (false, "Task not found.");
        }

        var task = await Db.Tasks
            .Include(t => t.Subtasks)
            .Include(t => t.TimeEntries)
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == model.Id.Value, ct);

        if (task is null)
        {
            return (false, "Task not found.");
        }

        var validation = await ValidateFormAsync(model, task.Id, ct);
        if (validation is not null)
        {
            return (false, validation);
        }

        if (task.Subtasks.Count > 0 && model.ProgressMode != task.ProgressMode)
        {
            return (false, "It cannot change: the task already has subtasks.");
        }

        var projectChanged = task.ProjectId != model.ProjectId;
        if (projectChanged)
        {
            var started = task.Status != WorkTaskStatus.ToDo || task.TimeEntries.Count > 0;
            if (started)
            {
                return (false, "The project cannot change once the task has started.");
            }

            foreach (var subtask in task.Subtasks)
            {
                subtask.ProjectId = model.ProjectId;
            }

            var dependencies = await Db.TaskDependencies.Where(d => d.TaskId == task.Id).ToListAsync(ct);
            Db.TaskDependencies.RemoveRange(dependencies);
        }

        task.Title = model.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        task.ProjectId = model.ProjectId;
        task.AssigneeId = model.AssigneeId;
        task.Priority = model.Priority;
        task.Tag = string.IsNullOrWhiteSpace(model.Tag) ? null : model.Tag;
        task.StartDate = model.StartDate;
        task.DueDate = model.DueDate;
        task.EstimateHours = model.EstimateHours;
        task.StoryPoints = model.StoryPoints;
        task.InBacklog = model.InBacklog;
        task.ProgressMode = model.ProgressMode;

        var currentDependencies = await Db.TaskDependencies.Where(d => d.TaskId == task.Id).ToListAsync(ct);
        Db.TaskDependencies.RemoveRange(currentDependencies);
        foreach (var dependencyId in model.DependsOn.Distinct())
        {
            Db.TaskDependencies.Add(new TaskDependency { TaskId = task.Id, DependsOnTaskId = dependencyId });
        }

        AddActivity(task.ProjectId, actorId, ActivityVerb.Moved, task.Id, $"Task \"{task.Title}\" edited");
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<MoveTaskResult> MoveAsync(Guid taskId, WorkTaskStatus status, Guid actorId, CancellationToken ct = default)
    {
        var task = await Db.Tasks
            .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
            .Include(t => t.Subtasks)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task is null)
        {
            return new MoveTaskResult(false, "Task not found.", null, null, null);
        }

        if (task.Status == status)
        {
            return new MoveTaskResult(true, null, null, null, null);
        }

        var blockers = BlockingRule.PendingBlockers(task.Dependencies.Select(d =>
            new Blocker(d.DependsOnTask?.Title ?? "?", d.DependsOnTask?.Status ?? WorkTaskStatus.ToDo, d.DependsOnTask?.ApprovedAt)));

        if (blockers.Count > 0 && TaskTransitions.RequiresUnblocked(status))
        {
            var message = _t.Format("It cannot start yet; it waits for: {0}", DescribeBlockers(blockers));
            return new MoveTaskResult(false, message, null, null, null);
        }

        var wasCompleted = task.Status == WorkTaskStatus.Completed;
        if (wasCompleted && status != WorkTaskStatus.Completed)
        {
            task.ReopenedCount += 1;
            task.CompletedAt = null;
            task.ApprovedAt = null;
            task.ApprovedByUserId = null;
        }

        task.Status = status;
        if (status == WorkTaskStatus.Completed)
        {
            task.CompletedAt = Clock.UtcNow;
            if (await CanApproveAsync(task.ProjectId, ct))
            {
                task.ApprovedAt = Clock.UtcNow;
                task.ApprovedByUserId = actorId;
            }
        }

        var verb = status == WorkTaskStatus.Completed ? ActivityVerb.MarkedDone : ActivityVerb.Moved;
        var text = status == WorkTaskStatus.Completed
            ? $"Task \"{task.Title}\" marked done"
            : $"Task \"{task.Title}\" moved to {ActivityStatus(status)}";
        AddActivity(task.ProjectId, actorId, verb, task.Id, text);

        if (task.AssigneeId.HasValue)
        {
            Notify(task.AssigneeId.Value, NotificationType.TaskMoved, text, actorId, task.Id, task.ProjectId);
        }

        await Db.SaveChangesAsync(ct);
        var card = await GetCardAsync(taskId, ct);
        return new MoveTaskResult(true, null, card, null, status == WorkTaskStatus.Completed ? _t["Task marked as completed"] : _t.Format("Task moved to {0}", _t[StatusLabels.Of(status)]));
    }

    public async Task<MoveTaskResult> ApproveAsync(Guid taskId, Guid actorId, CancellationToken ct = default)
    {
        var task = await Db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (task is null)
        {
            return new MoveTaskResult(false, "Task not found.", null, null, null);
        }

        if (!await CanApproveAsync(task.ProjectId, ct))
        {
            return new MoveTaskResult(false, "You do not have permission to approve this task.", null, null, null);
        }

        task.Status = WorkTaskStatus.Completed;
        task.CompletedAt ??= Clock.UtcNow;
        task.ApprovedAt = Clock.UtcNow;
        task.ApprovedByUserId = actorId;
        AddActivity(task.ProjectId, actorId, ActivityVerb.Approved, task.Id, $"Task \"{task.Title}\" approved");
        await Db.SaveChangesAsync(ct);
        return new MoveTaskResult(true, null, await GetCardAsync(taskId, ct), null, "Task approved");
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await Db.Tasks.Include(t => t.Subtasks).FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (task is null)
        {
            return (false, "Task not found.");
        }

        var ids = task.Subtasks.Select(s => s.Id).Append(task.Id).ToList();
        var dependencies = await Db.TaskDependencies.Where(d => ids.Contains(d.TaskId) || ids.Contains(d.DependsOnTaskId)).ToListAsync(ct);
        Db.TaskDependencies.RemoveRange(dependencies);
        Db.Tasks.RemoveRange(await Db.Tasks.Where(t => ids.Contains(t.Id)).ToListAsync(ct));
        AddActivity(task.ProjectId, User.UserId, ActivityVerb.Created, null, $"Task \"{task.Title}\" deleted");
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> AddSubtaskAsync(Guid parentId, string title, decimal hours, Guid? assigneeId, Guid actorId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return (false, "The subtask needs a title.");
        }

        var parent = await Db.Tasks.FirstOrDefaultAsync(t => t.Id == parentId, ct);
        if (parent is null)
        {
            return (false, "Task not found.");
        }

        if (parent.ParentId.HasValue)
        {
            return (false, "A subtask cannot have subtasks.");
        }

        Db.Tasks.Add(new WorkTask
        {
            WorkspaceId = WorkspaceId,
            ProjectId = parent.ProjectId,
            ParentId = parent.Id,
            Title = title.Trim(),
            AssigneeId = assigneeId,
            EstimateHours = hours,
            InBacklog = parent.InBacklog,
            Status = WorkTaskStatus.ToDo,
            ProgressMode = TaskProgressMode.Hours
        });

        AddActivity(parent.ProjectId, actorId, ActivityVerb.Created, parent.Id, $"Subtask \"{title.Trim()}\" created");
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> AddChecklistStepAsync(Guid taskId, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (false, "Write a step first.");
        }

        var count = await Db.ChecklistSteps.CountAsync(c => c.TaskId == taskId, ct);
        Db.ChecklistSteps.Add(new ChecklistStep { TaskId = taskId, Text = text.Trim(), Position = count });
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> ToggleChecklistStepAsync(Guid stepId, CancellationToken ct = default)
    {
        var step = await Db.ChecklistSteps.FirstOrDefaultAsync(c => c.Id == stepId, ct);
        if (step is null)
        {
            return (false, "Step not found.");
        }

        step.Done = !step.Done;
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> DeleteChecklistStepAsync(Guid stepId, CancellationToken ct = default)
    {
        var step = await Db.ChecklistSteps.FirstOrDefaultAsync(c => c.Id == stepId, ct);
        if (step is null)
        {
            return (false, "Step not found.");
        }

        Db.ChecklistSteps.Remove(step);
        await Db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<AvailabilityWarning?> CheckAvailabilityAsync(Guid? assigneeId, DateOnly? start, DateOnly? due, decimal hours, Guid? excludeTaskId, CancellationToken ct = default)
    {
        if (!assigneeId.HasValue || !start.HasValue || !due.HasValue || due < start)
        {
            return null;
        }

        var user = await Db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == assigneeId.Value, ct);
        if (user is null)
        {
            return null;
        }

        var days = Math.Max(1, due.Value.DayNumber - start.Value.DayNumber + 1);
        var capacity = user.CapacityHoursPerWeek / 5m * days;

        var booked = await Db.Tasks.AsNoTracking()
            .Where(t => t.AssigneeId == assigneeId.Value && t.Status != WorkTaskStatus.Completed &&
                (excludeTaskId == null || t.Id != excludeTaskId))
            .Where(t => t.StartDate == null || t.DueDate == null || (t.StartDate <= due && t.DueDate >= start))
            .SumAsync(t => t.EstimateHours, ct);

        if (booked + hours > capacity)
        {
            return new AvailabilityWarning(_t.Format(
                "{0} has {1}h free in that range and already has {2}h booked.",
                user.FullName,
                capacity.ToString("0.#", CultureInfo.CurrentCulture),
                booked.ToString("0.#", CultureInfo.CurrentCulture)));
        }

        return null;
    }

    public async Task<IReadOnlyList<SearchHitDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var term = query.Trim();
        var hits = new List<SearchHitDto>();

        var tasks = await Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId && t.Title.Contains(term))
            .OrderBy(t => t.Title)
            .Take(6)
            .Select(t => new { t.Id, t.Title, t.ParentId })
            .ToListAsync(ct);

        hits.AddRange(tasks.Select(t => new SearchHitDto(t.Title, "Task", $"/board?task={t.Id}", t.ParentId.HasValue ? "Subtask" : null)));

        var projects = await VisibleProjects()
            .Where(p => p.Name.Contains(term) || p.Code.Contains(term))
            .Take(4)
            .Select(p => new { p.Id, p.Code, p.Name })
            .ToListAsync(ct);

        hits.AddRange(projects.Select(p => new SearchHitDto($"{p.Code} · {p.Name}", "Project", $"/projects/{p.Id}", null)));

        var people = await Db.Users.AsNoTracking()
            .Where(u => u.WorkspaceId == WorkspaceId && u.FullName.Contains(term))
            .Take(4)
            .Select(u => new { u.Id, u.FullName, u.JobTitle })
            .ToListAsync(ct);

        hits.AddRange(people.Select(p => new SearchHitDto(p.FullName, "Person", $"/team", p.JobTitle)));

        return hits;
    }

    public Task<int> CountActiveAsync(CancellationToken ct = default) =>
        Db.Tasks.CountAsync(t => t.WorkspaceId == WorkspaceId && t.Status != WorkTaskStatus.Completed, ct);

    private IQueryable<WorkTask> LoadTaskGraph() =>
        Db.Tasks.AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Parent)
            .Include(t => t.Subtasks).ThenInclude(s => s.TimeEntries)
            .Include(t => t.TimeEntries)
            .Include(t => t.Dependencies).ThenInclude(d => d.DependsOnTask)
            .Include(t => t.Checklist)
            .Where(t => t.WorkspaceId == WorkspaceId);

    private TaskCardDto BuildCard(WorkTask task, Dictionary<Guid, ApplicationUser> users, Guid? activeTimerTaskId)
    {
        var weights = task.Subtasks.Select(s => new SubtaskWeight(s.EstimateHours, s.Status == WorkTaskStatus.Completed)).ToList();
        var blockers = BlockingRule.PendingBlockers(task.Dependencies.Select(d =>
            new Blocker(d.DependsOnTask?.Title ?? "?", d.DependsOnTask?.Status ?? WorkTaskStatus.ToDo, d.DependsOnTask?.ApprovedAt)));

        var loggedSeconds = task.TimeEntries.Sum(e => e.Seconds) + task.Subtasks.Sum(s => s.TimeEntries.Sum(e => e.Seconds));
        var overdue = task.DueDate.HasValue && task.DueDate.Value < Clock.Today && task.Status != WorkTaskStatus.Completed;

        return new TaskCardDto(
            task.Id,
            task.Title,
            task.ProjectId,
            task.Project?.Code,
            task.Project?.Name,
            task.Parent?.Title,
            task.Status,
            StatusLabels.Of(task.Status),
            task.Priority,
            StatusLabels.Of(task.Priority),
            task.Tag,
            PersonOf(users, task.AssigneeId),
            task.DueDate,
            overdue,
            blockers.Count > 0,
            blockers.Select(b => $"{b.TaskTitle} · {_t[BlockingRule.StatusLabel(b.Status)]}").ToList(),
            ProgressCalculator.Calculate(task.ProgressMode, weights, task.Status == WorkTaskStatus.Completed),
            task.Subtasks.Count(s => s.Status == WorkTaskStatus.Completed),
            task.Subtasks.Count,
            TimeSpan.FromSeconds(loggedSeconds),
            task.EstimateHours,
            activeTimerTaskId == task.Id,
            BuildSubtaskLines(task, users),
            BuildAssignedTeam(task, users),
            task.Checklist.Count(c => c.Done),
            task.Checklist.Count,
            task.StoryPoints,
            task.IsSubtask,
            task.Status == WorkTaskStatus.Completed && task.ApprovedAt is null);
    }

    private IReadOnlyList<SubtaskLine> BuildSubtaskLines(WorkTask task, Dictionary<Guid, ApplicationUser> users)
    {
        var totalHours = task.Subtasks.Sum(s => Math.Max(0m, s.EstimateHours));
        return task.Subtasks
            .OrderBy(s => s.CreatedAt)
            .Select(s =>
            {
                var weight = totalHours > 0
                    ? (int)Math.Round(Math.Max(0m, s.EstimateHours) * 100m / totalHours, MidpointRounding.AwayFromZero)
                    : (task.Subtasks.Count > 0 ? (int)Math.Round(100d / task.Subtasks.Count, MidpointRounding.AwayFromZero) : 0);
                return new SubtaskLine(
                    s.Id,
                    s.Title,
                    s.Description,
                    s.Status,
                    StatusLabels.Of(s.Status),
                    s.EstimateHours,
                    weight,
                    PersonOf(users, s.AssigneeId),
                    TimeSpan.FromSeconds(s.TimeEntries.Sum(e => e.Seconds)),
                    s.DueDate.HasValue && s.DueDate.Value < Clock.Today && s.Status != WorkTaskStatus.Completed);
            })
            .ToList();
    }

    private IReadOnlyList<PersonChip> BuildAssignedTeam(WorkTask task, Dictionary<Guid, ApplicationUser> users)
    {
        var ids = task.Subtasks.Select(s => s.AssigneeId)
            .Append(task.AssigneeId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct();

        return ids.Select(id => PersonOf(users, id)).Where(p => p is not null).Select(p => p!).ToList();
    }

    private string DescribeBlockers(IReadOnlyList<Blocker> blockers) =>
        string.Join(" · ", blockers.Select(b => $"{b.TaskTitle} · {_t[BlockingRule.StatusLabel(b.Status)]}"));

    private async Task<IReadOnlyList<TaskDependencyOption>> BuildDependencyOptionsAsync(Guid? taskId, IReadOnlyList<Guid> exclude, CancellationToken ct)
    {
        var tasks = await Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId && (taskId == null || t.Id != taskId))
            .Where(t => !exclude.Contains(t.Id))
            .Include(t => t.Parent)
            .OrderBy(t => t.Title)
            .Take(200)
            .ToListAsync(ct);

        var selected = taskId.HasValue
            ? await Db.TaskDependencies.Where(d => d.TaskId == taskId.Value).Select(d => d.DependsOnTaskId).ToListAsync(ct)
            : [];

        return tasks.Select(t => new TaskDependencyOption(
            t.Id,
            t.Parent is null ? t.Title : $"{t.Parent.Title} › {t.Title}",
            selected.Contains(t.Id))).ToList();
    }

    private async Task<string?> ValidateFormAsync(TaskFormModel model, Guid? taskId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            return "The title is required.";
        }

        if (model.StartDate.HasValue && model.DueDate.HasValue && model.StartDate > model.DueDate)
        {
            return "The start date cannot be after the due date.";
        }

        if (model.EstimateHours < 0 || model.StoryPoints < 0)
        {
            return "Estimate and story points must be positive.";
        }

        var edges = await Db.TaskDependencies.AsNoTracking()
            .Where(d => d.Task!.WorkspaceId == WorkspaceId)
            .Select(d => new ValueTuple<Guid, Guid>(d.TaskId, d.DependsOnTaskId))
            .ToListAsync(ct);

        var selfId = taskId ?? Guid.NewGuid();
        foreach (var dependency in model.DependsOn.Distinct())
        {
            if (dependency == taskId)
            {
                return "A task cannot depend on itself.";
            }

            if (DependencyCycleDetector.WouldCreateCycle(selfId, dependency, edges))
            {
                return "This dependency would create a cycle.";
            }
        }

        return null;
    }

    private async Task<bool> CanApproveAsync(Guid? projectId, CancellationToken ct)
    {
        if (User.IsAdmin)
        {
            return true;
        }

        if (!projectId.HasValue)
        {
            return false;
        }

        return await Db.Projects.AsNoTracking()
            .AnyAsync(p => p.Id == projectId.Value && p.LeadUserId == User.UserId, ct);
    }

    private static string ActivityStatus(WorkTaskStatus status) => status switch
    {
        WorkTaskStatus.ToDo => "To do",
        WorkTaskStatus.InProgress => "In progress",
        WorkTaskStatus.Review => "In review",
        _ => "Completed"
    };
}
