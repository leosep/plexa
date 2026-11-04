using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record CalendarEventDto(
    Guid Id,
    string Title,
    DateOnly Date,
    TimeOnly StartsAt,
    TimeOnly EndsAt,
    CalendarEventKind Kind,
    string KindLabel,
    string? Place,
    Guid? ProjectId,
    string? ProjectLabel,
    IReadOnlyList<string> Members,
    IReadOnlyList<string> Departments,
    string TimeLabel,
    string DateLabel);

public sealed record CalendarDayDto(
    DateOnly Date,
    bool IsToday,
    bool IsCurrentMonth,
    int Count,
    IReadOnlyList<CalendarEventDto> Events);

public sealed record CalendarDto(
    string View,
    string RangeLabel,
    int EventCount,
    IReadOnlyList<CalendarDayDto> Days,
    DateOnly Anchor,
    string PrevUrl,
    string NextUrl,
    string TodayUrl);

public sealed class CalendarEventFormModel
{
    public string Title { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartsAt { get; set; } = new(10, 0);
    public TimeOnly EndsAt { get; set; } = new(11, 0);
    public CalendarEventKind Kind { get; set; } = CalendarEventKind.Meeting;
    public string? Place { get; set; }
    public Guid? ProjectId { get; set; }
    public List<Guid> MemberIds { get; set; } = [];
    public List<Guid> DepartmentIds { get; set; } = [];
}

