using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Entities;
using Plexa.Domain.Enums;
using Plexa.Domain.Rules;

namespace Plexa.Infrastructure.Services;

public sealed class TimerService : ServiceBase, ITimerService
{
    public TimerService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<TimerDto?> GetActiveAsync(Guid userId, CancellationToken ct = default)
    {
        var entry = await Db.TimeEntries.AsNoTracking()
            .Include(e => e.Task)
            .Where(e => e.UserId == userId && e.EndedAt == null)
            .FirstOrDefaultAsync(ct);

        return entry is null ? null : ToDto(entry);
    }

    public async Task<TimerDto> StartAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await Db.Tasks.Include(t => t.TimeEntries).FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new InvalidOperationException("Task not found.");

        await StopActiveAsync(userId, ct);

        var entry = new TimeEntry
        {
            TaskId = taskId,
            UserId = userId,
            StartedAt = Clock.UtcNow
        };

        Db.TimeEntries.Add(entry);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Otro start simultáneo ganó la carrera: se detiene y se reintenta una vez.
            Db.Entry(entry).State = EntityState.Detached;
            await StopActiveAsync(userId, ct);
            Db.TimeEntries.Add(entry);
            await Db.SaveChangesAsync(ct);
        }

        entry.Task = task;
        return ToDto(entry);
    }

    public async Task<TimerDto?> StopAsync(Guid userId, CancellationToken ct = default)
    {
        var entry = await Db.TimeEntries
            .Include(e => e.Task)
            .Where(e => e.UserId == userId && e.EndedAt == null)
            .FirstOrDefaultAsync(ct);

        if (entry is null)
        {
            return null;
        }

        var dto = ToDto(entry);
        entry.EndedAt = Clock.UtcNow;
        entry.Seconds = Math.Max(1, (int)(entry.EndedAt.Value - entry.StartedAt).TotalSeconds);
        await Db.SaveChangesAsync(ct);
        return dto;
    }

    public async Task<TodayTrackedDto> GetTodayAsync(Guid userId, CancellationToken ct = default)
    {
        var today = Clock.Today;
        var startUtc = today.ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        var entries = await Db.TimeEntries.AsNoTracking()
            .Include(e => e.Task)
            .Where(e => e.UserId == userId && e.StartedAt >= startUtc)
            .ToListAsync(ct);

        var total = TimeSpan.FromSeconds(entries.Sum(e => e.Seconds));
        var active = entries.FirstOrDefault(e => e.EndedAt is null);

        if (active is not null)
        {
            total = TimeSpan.FromSeconds(entries.Where(e => e.EndedAt is not null).Sum(e => e.Seconds)
                + (Clock.UtcNow - active.StartedAt).TotalSeconds);
        }

        var activeDto = active is null ? null : ToDto(active);
        var logged = active?.Task is null
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(active.Task.TimeEntries.Where(e => e.Id != active.Id).Sum(e => e.Seconds));

        return new TodayTrackedDto(total, activeDto, activeDto?.TaskTitle, active?.Task?.EstimateHours, logged);
    }

    private async Task StopActiveAsync(Guid userId, CancellationToken ct)
    {
        var active = await Db.TimeEntries
            .Where(e => e.UserId == userId && e.EndedAt == null)
            .ToListAsync(ct);

        if (active.Count == 0)
        {
            return;
        }

        foreach (var entry in active)
        {
            entry.EndedAt = Clock.UtcNow;
            entry.Seconds = Math.Max(1, (int)(entry.EndedAt.Value - entry.StartedAt).TotalSeconds);
        }

        await Db.SaveChangesAsync(ct);
    }

    private TimerDto ToDto(TimeEntry entry)
    {
        var elapsed = entry.EndedAt.HasValue
            ? entry.Seconds
            : Math.Max(0, (int)(Clock.UtcNow - entry.StartedAt).TotalSeconds);

        var logged = entry.Task is null
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(entry.Task.TimeEntries.Where(e => e.Id != entry.Id).Sum(e => e.Seconds));

        return new TimerDto(
            entry.TaskId,
            entry.Task?.Title ?? "Task",
            entry.StartedAt,
            elapsed,
            entry.Task?.EstimateHours ?? 0m,
            logged);
    }
}

public sealed class CapacityService : ServiceBase, ICapacityService
{
    public CapacityService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<IReadOnlyList<MemberCapacityDto>> GetTeamAsync(CancellationToken ct = default)
    {
        var users = await Db.Users.AsNoTracking()
            .Where(u => u.WorkspaceId == WorkspaceId)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        var sprint = await Db.Sprints.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WorkspaceId == WorkspaceId && s.Status == SprintStatus.Active, ct);

        var today = Clock.Today;
        var result = new List<MemberCapacityDto>();

        foreach (var user in users)
        {
            var tasks = await Db.Tasks.AsNoTracking()
                .Where(t => t.WorkspaceId == WorkspaceId && t.AssigneeId == user.Id && t.Status != WorkTaskStatus.Completed)
                .ToListAsync(ct);

            var overdue = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < today);
            var openHours = tasks.Sum(t => t.EstimateHours);

            var capacityHours = 0m;
            if (sprint is not null)
            {
                var days = Math.Max(1, sprint.EndDate.DayNumber - sprint.StartDate.DayNumber + 1);
                capacityHours = user.CapacityHoursPerWeek / 7m * days;
            }

            var percent = CapacityCalculator.Percent(openHours, capacityHours);
            var signal = CapacityCalculator.Signal(percent, overdue, sprint is not null);

            result.Add(new MemberCapacityDto(
                user.ToPerson(),
                tasks.Count,
                overdue,
                percent,
                signal,
                CapacityCalculator.SignalLabel(signal),
                CapacityCalculator.AvailabilityLabel(signal),
                openHours,
                capacityHours));
        }

        return result;
    }
}

public sealed class DashboardService : ServiceBase, IDashboardService
{
    private readonly ICapacityService _capacity;
    private readonly ITimerService _timer;

    public DashboardService(PlexaDbContext db, IUserContext user, IClock clock, ICapacityService capacity, ITimerService timer)
        : base(db, user, clock)
    {
        _capacity = capacity;
        _timer = timer;
    }

    public async Task<DashboardDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var today = Clock.Today;
        var activeSprint = await Db.Sprints.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WorkspaceId == WorkspaceId && s.Status == SprintStatus.Active, ct);

        // Ámbito del dashboard: sprint activo o backlog (InBacklog).
        var scope = Db.Tasks.AsNoTracking().Where(t => t.WorkspaceId == WorkspaceId && t.InBacklog);
        if (activeSprint is not null)
        {
            scope = Db.Tasks.AsNoTracking().Where(t => t.WorkspaceId == WorkspaceId && t.SprintId == activeSprint.Id);
        }

        var tasks = await scope.ToListAsync(ct);

        var activeTasks = tasks.Count(t => t.Status != WorkTaskStatus.Completed);
        var dueToday = tasks.Count(t => t.DueDate == today && t.Status != WorkTaskStatus.Completed);
        var completed = tasks.Count(t => t.Status == WorkTaskStatus.Completed);
        var overdue = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < today && t.Status != WorkTaskStatus.Completed);

        var closedSince = Clock.UtcNow.AddDays(-7);
        var closedPrev = Clock.UtcNow.AddDays(-14);
        var closedThisWeek = await Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId && t.CompletedAt != null && t.CompletedAt >= closedSince)
            .SumAsync(t => t.EstimateHours, ct);
        var closedPrevWeek = await Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId && t.CompletedAt != null && t.CompletedAt >= closedPrev && t.CompletedAt < closedSince)
            .SumAsync(t => t.EstimateHours, ct);

        var trend = closedPrevWeek <= 0
            ? (closedThisWeek > 0 ? "▲ +100% vs prev. week" : "— vs prev. week")
            : $"{(closedThisWeek >= closedPrevWeek ? "▲ +" : "▼ ")}{{0}}% vs prev. week|{Math.Round((closedThisWeek - closedPrevWeek) * 100m / closedPrevWeek, 0)}";

        var kpis = new List<KpiCard>
        {
            KpiCard.Neutral("Active tasks", activeTasks.ToString(), $"{{0}} due today|{dueToday}"),
            KpiCard.Success("Completed this sprint", $"{completed} / {tasks.Count}", $"{{0}}% of sprint|{Math.Round(completed * 100d / tasks.Count)}"),
            KpiCard.Neutral("Hours closed (7d)", DurationFormatter.FormatShort(TimeSpan.FromHours((double)closedThisWeek)), null, trend),
            overdue > 0 ? KpiCard.Danger("Overdue", overdue.ToString(), "Needs attention") : KpiCard.Success("Overdue", "0", "On track")
        };

        var distribution = new[] { WorkTaskStatus.ToDo, WorkTaskStatus.InProgress, WorkTaskStatus.Review, WorkTaskStatus.Completed }
            .Select(status =>
            {
                var count = tasks.Count(t => t.Status == status);
                return new StatusSlice(status, StatusLabels.Of(status), count,
                    tasks.Count == 0 ? 0 : (int)Math.Round(count * 100d / tasks.Count));
            })
            .ToList();

        var heatmap = await BuildHeatmapAsync(ct);
        var heatmapActions = heatmap.Sum(w => w.Days.Sum());

        var openTasks = tasks.Where(t => t.Status != WorkTaskStatus.Completed).ToList();
        var priorities = new[] { TaskPriority.High, TaskPriority.Medium, TaskPriority.Low }
            .Select(p => new PrioritySlice(p, StatusLabels.Of(p), openTasks.Count(t => t.Priority == p)))
            .ToList();

        var tracked = await _timer.GetTodayAsync(userId, ct);
        var workload = await _capacity.GetTeamAsync(ct);

        var greeting = GreetingFor(Clock.UtcNow.ToLocalTime().Hour, User.FullName);
        var context = new List<string>
        {
            activeSprint is null ? "No active sprint." : $"Sprint “{{0}}” is active.|{activeSprint.Name}"
        };
        if (overdue > 0)
        {
            context.Add($"{{0}} tasks are past their deadline.|{overdue}");
        }

        context.Add(today.ToString("ddd dd MMM", System.Globalization.CultureInfo.CurrentCulture));

        return new DashboardDto(
            greeting,
            context,
            kpis,
            distribution,
            tasks.Count,
            heatmap,
            heatmapActions,
            priorities,
            openTasks.Count,
            tracked,
            workload,
            today.ToString("ddd dd MMM", System.Globalization.CultureInfo.CurrentCulture));
    }

    private async Task<IReadOnlyList<HeatmapWeek>> BuildHeatmapAsync(CancellationToken ct)
    {
        var today = Clock.Today;
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var start = monday.AddDays(-21);

        var since = start.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var logs = await Db.ActivityLogs.AsNoTracking()
            .Where(a => a.WorkspaceId == WorkspaceId && a.CreatedAt >= since)
            .Select(a => a.CreatedAt)
            .ToListAsync(ct);

        var weeks = new List<HeatmapWeek>();
        var labels = new[] { "-3 wk", "-2 wk", "-1 wk", "This wk" };

        for (var week = 0; week < 4; week++)
        {
            var weekStart = start.AddDays(week * 7);
            var days = new List<int>();
            for (var day = 0; day < 7; day++)
            {
                var date = weekStart.AddDays(day);
                days.Add(logs.Count(l => DateOnly.FromDateTime(l.ToLocalTime()) == date));
            }

            weeks.Add(new HeatmapWeek(labels[week], days));
        }

        return weeks;
    }

    private static string GreetingFor(int hour, string fullName)
    {
        var part = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        return $"{part}, {{0}}|{fullName}";
    }
}
