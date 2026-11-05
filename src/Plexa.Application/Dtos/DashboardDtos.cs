using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record TimerDto(Guid TaskId, string TaskTitle, DateTime StartedAt, int ElapsedSeconds, decimal EstimateHours, TimeSpan Logged);

public sealed record TodayTrackedDto(TimeSpan Total, TimerDto? Active, string? ActiveTaskTitle, decimal? EstimateHours, TimeSpan LoggedToday);

public sealed record StatusSlice(WorkTaskStatus Status, string Label, int Count, int Percent);

public sealed record HeatmapWeek(string Label, IReadOnlyList<int> Days);

public sealed record PrioritySlice(TaskPriority Priority, string Label, int Count);

public sealed record DashboardDto(
    string Greeting,
    IReadOnlyList<string> ContextItems,
    IReadOnlyList<KpiCard> Kpis,
    IReadOnlyList<StatusSlice> StatusDistribution,
    int BacklogCount,
    IReadOnlyList<HeatmapWeek> Heatmap,
    int HeatmapActions,
    IReadOnlyList<PrioritySlice> Priorities,
    int OpenTasks,
    TodayTrackedDto TrackedToday,
    IReadOnlyList<MemberCapacityDto> Workload,
    string TodayLabel);

public sealed record MemberCapacityDto(
    PersonChip Person,
    int ActiveTasks,
    int OverdueTasks,
    int CapacityPercent,
    CapacitySignal Signal,
    string SignalLabel,
    string Availability,
    decimal OpenHours,
    decimal CapacityHours);
