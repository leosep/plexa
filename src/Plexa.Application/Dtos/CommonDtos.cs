using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record PersonChip(Guid Id, string FullName, string Initials, string? JobTitle = null, string? AvatarPath = null)
{
    public static PersonChip From(Guid id, string name, string? jobTitle = null, string? avatarPath = null)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => string.Concat(parts[0][..1], parts[^1][..1]).ToUpperInvariant()
        };
        return new PersonChip(id, name, initials, jobTitle, avatarPath);
    }
}

/// <summary>Datos de la cabecera y la tarjeta de sprint de la barra lateral.</summary>
public sealed record WorkspaceSidebarDto(
    string Name,
    string? SprintName,
    string? SprintRange,
    int CommittedPoints,
    int CapacityPoints,
    int DaysLeft)
{
    public bool HasSprint => !string.IsNullOrWhiteSpace(SprintName);
}
public sealed record ProjectOption(Guid Id, string Code, string Name, string Label);

public sealed record StatusOption(WorkTaskStatus Value, string Label);

public sealed record TagOption(string Value, string Label);

public sealed record ToastMessage(string Type, string Message, string? UndoUrl = null);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int Pages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < Pages;
}

public sealed record KpiCard(string Label, string Value, string? Context = null, string Tone = "neutral", string? Trend = null)
{
    public static KpiCard Neutral(string label, string value, string? context = null, string? trend = null) =>
        new(label, value, context, "neutral", trend);

    public static KpiCard Danger(string label, string value, string? context = null, string? trend = null) =>
        new(label, value, context, "danger", trend);

    public static KpiCard Success(string label, string value, string? context = null, string? trend = null) =>
        new(label, value, context, "success", trend);
}

public static class StatusLabels
{
    public static string Of(WorkTaskStatus status) => status switch
    {
        WorkTaskStatus.ToDo => "To do",
        WorkTaskStatus.InProgress => "In progress",
        WorkTaskStatus.Review => "Review",
        _ => "Completed"
    };

    public static string Of(ProjectStatus status) => status switch
    {
        ProjectStatus.Planning => "Planning",
        ProjectStatus.InProgress => "In progress",
        ProjectStatus.AtRisk => "At risk",
        ProjectStatus.OnHold => "On hold",
        ProjectStatus.Completed => "Completed",
        _ => "Archived"
    };

    public static string Of(ProjectHealth health) => health == ProjectHealth.AtRisk ? "At risk" : "On track";

    public static string Of(TaskPriority priority) => priority switch
    {
        TaskPriority.High => "High",
        TaskPriority.Low => "Low",
        _ => "Medium"
    };

    public static string Of(CalendarEventKind kind) => kind switch
    {
        CalendarEventKind.Meeting => "Meeting",
        CalendarEventKind.Review => "Review",
        CalendarEventKind.Demo => "Demo",
        CalendarEventKind.Deadline => "Deadline",
        CalendarEventKind.Retro => "Retro",
        _ => "Other"
    };

    public static string Of(SprintStatus status) => status switch
    {
        SprintStatus.Planned => "Planned",
        SprintStatus.Active => "Active",
        _ => "Closed"
    };
}

public static class Tags
{
    public static readonly IReadOnlyList<string> All =
    [
        "Backend", "Database", "Design", "Frontend", "Research", "QA", "Performance"
    ];
}
