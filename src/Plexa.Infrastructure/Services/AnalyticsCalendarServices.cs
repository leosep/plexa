using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Enums;
using Plexa.Domain.Rules;

namespace Plexa.Infrastructure.Services;

public sealed class AnalyticsService : ServiceBase, IAnalyticsService
{
    public AnalyticsService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<AnalyticsDto> GetAsync(CancellationToken ct = default)
    {
        var today = Clock.Today;
        var startThis = StartOfWeek(today).AddDays(-7 * 7);
        var startPrev = startThis.AddDays(-7 * 8);

        var tasks = await Db.Tasks.AsNoTracking()
            .Where(t => t.WorkspaceId == WorkspaceId)
            .Include(t => t.TimeEntries)
            .ToListAsync(ct);

        var closedThis = tasks.Where(t => t.CompletedAt.HasValue && DateOnly.FromDateTime(t.CompletedAt.Value.ToLocalTime()) >= startThis).ToList();
        var closedPrev = tasks.Where(t => t.CompletedAt.HasValue &&
            DateOnly.FromDateTime(t.CompletedAt.Value.ToLocalTime()) >= startPrev &&
            DateOnly.FromDateTime(t.CompletedAt.Value.ToLocalTime()) < startThis).ToList();

        var closedDelta = closedPrev.Count == 0
            ? (closedThis.Count > 0 ? "+100%" : "0%")
            : $"{Math.Round((closedThis.Count - closedPrev.Count) * 100d / closedPrev.Count)}%";

        var cycleThis = AverageCycleDays(closedThis);
        var cyclePrev = AverageCycleDays(closedPrev);

        var reopened = tasks.Count(t => t.ReopenedCount > 0);
        var reworkPercent = closedThis.Count == 0 ? 0 : (int)Math.Round(reopened * 100d / Math.Max(1, closedThis.Count));

        var withBoth = closedThis.Where(t => t.EstimateHours > 0 && t.TimeEntries.Sum(e => e.Seconds) > 0).ToList();
        var reliable = withBoth.Count(t =>
        {
            var logged = (decimal)t.TimeEntries.Sum(e => e.Seconds) / 3600m;
            var diff = Math.Abs(logged - t.EstimateHours);
            return t.EstimateHours > 0 && diff / t.EstimateHours <= 0.20m;
        });

        var closedSprints = await Db.Sprints.AsNoTracking()
            .Where(s => s.WorkspaceId == WorkspaceId && s.Status == SprintStatus.Closed)
            .OrderBy(s => s.EndDate)
            .Take(8)
            .ToListAsync(ct);

        var reliability = withBoth.Count == 0
            ? new AnalyticsKpiDto("Estimate reliability", "—", "within ±20%", true)
            : new AnalyticsKpiDto("Estimate reliability", $"{Math.Round(reliable * 100d / withBoth.Count)}%", "within ±20%", reliable * 2 >= withBoth.Count);

        var delivery = closedSprints.Count == 0
            ? new AnalyticsKpiDto("Delivery / plan", "—", "Target 1.00", true)
            : BuildDeliveryKpi(closedSprints, tasks);

        var cycleDelta = closedPrev.Count == 0 ? 0d : cycleThis - cyclePrev;
        var kpis = new List<AnalyticsKpiDto>
        {
            new("Tasks closed", closedThis.Count.ToString(CultureInfo.InvariantCulture), $"{(closedThis.Count >= closedPrev.Count ? "▲ +" : "▼ ")}{closedDelta} vs prev. 8w", closedThis.Count >= closedPrev.Count),
            new("Avg cycle time", $"{cycleThis:0.0}d", $"{(cycleDelta <= 0 ? "▼" : "▲")} {cycleDelta:+0.0;-0.0;0.0}d vs prev.", cycleDelta <= 0),
            new("Rework", $"{reworkPercent}%", $"▲ +{reopened} pts", reopened == 0),
            delivery,
            reliability
        };

        var tracked = tasks.Where(t => t.InBacklog).ToList();
        var shares = new[] { WorkTaskStatus.ToDo, WorkTaskStatus.InProgress, WorkTaskStatus.Review, WorkTaskStatus.Completed }
            .Select(status =>
            {
                var count = tracked.Count(t => t.Status == status);
                return new StatusSlice(status, StatusLabels.Of(status), count,
                    tracked.Count == 0 ? 0 : (int)Math.Round(count * 100d / tracked.Count));
            })
            .ToList();

        var labels = new List<string>();
        var todo = new List<int>();
        var progress = new List<int>();
        var review = new List<int>();
        var completed = new List<int>();

        for (var week = 0; week < 8; week++)
        {
            var weekStart = startThis.AddDays(week * 7);
            var weekEnd = weekStart.AddDays(6);
            labels.Add($"W{ISOWeek.GetWeekOfYear(weekStart.ToDateTime(TimeOnly.MinValue))}");

            var createdBefore = tracked.Where(t => DateOnly.FromDateTime(t.CreatedAt.ToLocalTime()) <= weekEnd).ToList();
            var completedBy = createdBefore.Count(t => t.CompletedAt.HasValue && DateOnly.FromDateTime(t.CompletedAt.Value.ToLocalTime()) <= weekEnd);
            completed.Add(completedBy);
            todo.Add(createdBefore.Count(t => t.Status == WorkTaskStatus.ToDo));
            progress.Add(createdBefore.Count(t => t.Status == WorkTaskStatus.InProgress));
            review.Add(createdBefore.Count(t => t.Status == WorkTaskStatus.Review));
        }

        var cumulative = new CumulativeFlowDto(
            labels,
            [
                new WeekSeriesDto("To do", todo),
                new WeekSeriesDto("In progress", progress),
                new WeekSeriesDto("Review", review),
                new WeekSeriesDto("Completed", completed)
            ],
            shares,
            tracked.Count);

        var throughput = new ThroughputDto(
            labels,
            Enumerable.Range(0, 8).Select(week =>
            {
                var weekStart = startThis.AddDays(week * 7);
                var weekEnd = weekStart.AddDays(6);
                return closedThis.Count(t =>
                {
                    var date = DateOnly.FromDateTime(t.CompletedAt!.Value.ToLocalTime());
                    return date >= weekStart && date <= weekEnd;
                });
            }).ToList());

        var accuracyGroups = withBoth
            .GroupBy(t => t.Tag ?? "None")
            .OrderBy(g => g.Key)
            .ToList();

        var accuracy = accuracyGroups.Count == 0
            ? new EstimateAccuracyDto([], [], [], true)
            : new EstimateAccuracyDto(
                accuracyGroups.Select(g => g.Key).ToList(),
                accuracyGroups.Select(g => g.Sum(t => t.EstimateHours)).ToList(),
                accuracyGroups.Select(g => (decimal)g.Sum(t => t.TimeEntries.Sum(e => e.Seconds)) / 3600m).ToList(),
                false);

        var users = await LoadUsersAsync(ct);
        var load = users.Values.OrderByDescending(u => tasks.Count(t => t.AssigneeId == u.Id && t.Status != WorkTaskStatus.Completed))
            .Select(u => new LoadBubbleDto(u.Id, u.FullName, u.ToPerson().Initials,
                tasks.Count(t => t.AssigneeId == u.Id && t.Status != WorkTaskStatus.Completed)))
            .ToList();

        var quarterStart = Clock.UtcNow.AddMonths(-3);
        var reopenedQuarter = tasks.Where(t => t.ReopenedCount > 0 && t.UpdatedAt >= quarterStart).ToList();

        var reworkOrigin = new ReworkOriginDto(
            reopenedQuarter.Count,
            reopenedQuarter.GroupBy(t => t.Tag ?? "None").Select(g => (g.Key, g.Count())).ToList());

        var velocity = closedSprints.Count == 0
            ? new SprintVelocityDto([], [], [], true)
            : new SprintVelocityDto(
                closedSprints.Select(s => s.Name).ToList(),
                closedSprints.Select(s => s.CommittedPoints).ToList(),
                closedSprints.Select(s => Math.Max(0, tracked.Count(t => t.SprintId == s.Id && t.Status == WorkTaskStatus.Completed))).ToList(),
                false);

        return new AnalyticsDto(kpis, cumulative, throughput, accuracy, load, reworkOrigin, velocity);
    }

    private static double AverageCycleDays(IReadOnlyList<Domain.Entities.WorkTask> tasks)
    {
        var withDates = tasks.Where(t => t.CompletedAt.HasValue).ToList();
        if (withDates.Count == 0)
        {
            return 0;
        }

        return withDates.Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays);
    }

    private static AnalyticsKpiDto BuildDeliveryKpi(IReadOnlyList<Domain.Entities.Sprint> closedSprints, IReadOnlyList<Domain.Entities.WorkTask> tasks)
    {
        var committed = closedSprints.Sum(s => s.CommittedPoints);
        if (committed == 0)
        {
            return new AnalyticsKpiDto("Delivery / plan", "—", "Target 1.00", true);
        }

        var sprintIds = closedSprints.Select(s => s.Id).ToHashSet();
        var delivered = tasks
            .Where(t => t.SprintId.HasValue && sprintIds.Contains(t.SprintId.Value) && t.Status == WorkTaskStatus.Completed)
            .Sum(t => t.StoryPoints);

        var ratio = delivered / (double)committed;
        return new AnalyticsKpiDto("Delivery / plan", ratio.ToString("0.00", CultureInfo.InvariantCulture), "Target 1.00", ratio >= 1);
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }
}

public sealed class CalendarService : ServiceBase, ICalendarService
{
    public CalendarService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<CalendarDto> GetAsync(string view, DateOnly? anchor, CancellationToken ct = default)
    {
        var normalized = view switch
        {
            "2weeks" => "2weeks",
            "month" => "month",
            _ => "week"
        };

        var today = Clock.Today;
        var baseDate = anchor ?? today;

        DateOnly start;
        DateOnly end;

        if (normalized == "month")
        {
            var first = new DateOnly(baseDate.Year, baseDate.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            start = StartOfWeek(first);
            end = StartOfWeek(last).AddDays(6);
        }
        else
        {
            start = StartOfWeek(baseDate);
            end = normalized == "2weeks" ? start.AddDays(13) : start.AddDays(6);
        }

        var events = await LoadEventsAsync(start, end, ct);

        var days = new List<CalendarDayDto>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var dayEvents = events.Where(e => e.Date == date).OrderBy(e => e.StartsAt).ToList();
            days.Add(new CalendarDayDto(
                date,
                date == today,
                normalized != "month" || date.Month == baseDate.Month,
                dayEvents.Count,
                dayEvents));
        }

        var count = events.Count;
        var rangeLabel = $"{start:dd MMM} – {end:dd MMM} · {count} {(count == 1 ? "event" : "events")}";

        var step = normalized == "month" ? 1 : normalized == "2weeks" ? 14 : 7;
        var nextAnchor = normalized == "month" ? baseDate.AddMonths(1) : baseDate.AddDays(step);
        var prevAnchor = normalized == "month" ? baseDate.AddMonths(-1) : baseDate.AddDays(-step);

        return new CalendarDto(
            normalized,
            rangeLabel,
            count,
            days,
            baseDate,
            BuildUrl(normalized, prevAnchor),
            BuildUrl(normalized, nextAnchor),
            BuildUrl(normalized, today));
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await LoadEventsAsync(from, to, ct);

    public async Task<CalendarEventDto?> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await Db.CalendarEvents.AsNoTracking()
            .Include(e => e.Members)
            .Include(e => e.Departments)
            .FirstOrDefaultAsync(e => e.Id == id && e.WorkspaceId == WorkspaceId, ct);

        if (entity is null)
        {
            return null;
        }

        var users = await LoadUsersAsync(ct);
        var departments = await Db.Departments.AsNoTracking().Where(d => d.WorkspaceId == WorkspaceId).ToDictionaryAsync(d => d.Id, d => d.Name, ct);
        var project = entity.ProjectId.HasValue
            ? await Db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == entity.ProjectId.Value, ct)
            : null;

        return ToDto(entity, users, departments, project is null ? null : $"{project.Code} · {project.Name}");
    }

    public async Task<(bool Ok, string? Error, Guid Id)> CreateAsync(CalendarEventFormModel model, Guid actorId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            return (false, "The title is required.", Guid.Empty);
        }

        if (model.EndsAt <= model.StartsAt)
        {
            return (false, "The end time must be after the start time.", Guid.Empty);
        }

        var entity = new Domain.Entities.CalendarEvent
        {
            WorkspaceId = WorkspaceId,
            Title = model.Title.Trim(),
            Date = model.Date,
            StartsAt = model.StartsAt,
            EndsAt = model.EndsAt,
            Kind = model.Kind,
            Place = string.IsNullOrWhiteSpace(model.Place) ? null : model.Place.Trim(),
            ProjectId = model.ProjectId,
            CreatedByUserId = actorId
        };

        foreach (var memberId in model.MemberIds.Distinct())
        {
            entity.Members.Add(new Domain.Entities.CalendarEventMember { UserId = memberId });
        }

        foreach (var departmentId in model.DepartmentIds.Distinct())
        {
            entity.Departments.Add(new Domain.Entities.CalendarEventDepartment { DepartmentId = departmentId });
        }

        Db.CalendarEvents.Add(entity);
        AddActivity(model.ProjectId, actorId, ActivityVerb.Created, null, $"Event \"{entity.Title}\" created");

        foreach (var memberId in model.MemberIds.Distinct())
        {
            Notify(memberId, NotificationType.Message, $"You are invited to \"{entity.Title}\" on {entity.Date:dd MMM}", actorId, projectId: model.ProjectId);
        }

        await Db.SaveChangesAsync(ct);
        return (true, null, entity.Id);
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetForProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var users = await LoadUsersAsync(ct);
        var departments = await Db.Departments.AsNoTracking().Where(d => d.WorkspaceId == WorkspaceId).ToDictionaryAsync(d => d.Id, d => d.Name, ct);
        var project = await Db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);

        var events = await Db.CalendarEvents.AsNoTracking()
            .Include(e => e.Members)
            .Include(e => e.Departments)
            .Where(e => e.WorkspaceId == WorkspaceId && e.ProjectId == projectId)
            .OrderBy(e => e.Date).ThenBy(e => e.StartsAt)
            .ToListAsync(ct);

        return events.Select(e => ToDto(e, users, departments, project is null ? null : $"{project.Code} · {project.Name}")).ToList();
    }

    private async Task<IReadOnlyList<CalendarEventDto>> LoadEventsAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var users = await LoadUsersAsync(ct);
        var departments = await Db.Departments.AsNoTracking()
            .Where(d => d.WorkspaceId == WorkspaceId)
            .ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        var projects = await Db.Projects.AsNoTracking()
            .Where(p => p.WorkspaceId == WorkspaceId)
            .ToDictionaryAsync(p => p.Id, p => $"{p.Code} · {p.Name}", ct);

        var events = await Db.CalendarEvents.AsNoTracking()
            .Include(e => e.Members)
            .Include(e => e.Departments)
            .Where(e => e.WorkspaceId == WorkspaceId && e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date).ThenBy(e => e.StartsAt)
            .ToListAsync(ct);

        return events
            .Where(e => IsVisible(e))
            .Select(e => ToDto(e, users, departments,
                e.ProjectId.HasValue && projects.TryGetValue(e.ProjectId.Value, out var label) ? label : null))
            .ToList();
    }

    private bool IsVisible(Domain.Entities.CalendarEvent entity)
    {
        if (User.IsAdmin)
        {
            return true;
        }

        if (entity.Members.Count == 0 && entity.Departments.Count == 0)
        {
            return true;
        }

        return entity.Members.Any(m => m.UserId == User.UserId);
    }

    private static CalendarEventDto ToDto(
        Domain.Entities.CalendarEvent entity,
        Dictionary<Guid, Identity.ApplicationUser> users,
        Dictionary<Guid, string> departments,
        string? projectLabel) =>
        new(
            entity.Id,
            entity.Title,
            entity.Date,
            entity.StartsAt,
            entity.EndsAt,
            entity.Kind,
            StatusLabels.Of(entity.Kind),
            entity.Place,
            entity.ProjectId,
            projectLabel,
            entity.Members.Select(m => users.TryGetValue(m.UserId, out var u) ? u.FullName : "Member").ToList(),
            entity.Departments.Select(d => departments.TryGetValue(d.DepartmentId, out var name) ? name : "Department").ToList(),
            $"{entity.StartsAt:HH:mm} – {entity.EndsAt:HH:mm}",
            entity.Date.ToString("dd MMM", CultureInfo.GetCultureInfo("en-US")));

    private static DateOnly StartOfWeek(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private static string BuildUrl(string view, DateOnly date) => $"/calendar?view={view}&date={date:yyyy-MM-dd}";
}
