using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record SubtaskLine(
    Guid Id,
    string Title,
    string? Description,
    WorkTaskStatus Status,
    string StatusLabel,
    decimal Hours,
    int WeightPercent,
    PersonChip? Assignee,
    TimeSpan Logged,
    bool Overdue);

public sealed record ChecklistItemDto(Guid Id, string Text, bool Done);

public sealed record TaskCardDto(
    Guid Id,
    string Title,
    Guid? ProjectId,
    string? ProjectCode,
    string? ProjectName,
    string? ParentTitle,
    WorkTaskStatus Status,
    string StatusLabel,
    TaskPriority Priority,
    string PriorityLabel,
    string? Tag,
    PersonChip? Assignee,
    DateOnly? DueDate,
    bool Overdue,
    bool Blocked,
    IReadOnlyList<string> BlockedBy,
    int ProgressPct,
    int SubtasksDone,
    int SubtasksTotal,
    TimeSpan Logged,
    decimal EstimateHours,
    bool TimerRunning,
    IReadOnlyList<SubtaskLine> Subtasks,
    IReadOnlyList<PersonChip> AssignedTeam,
    int ChecklistDone,
    int ChecklistTotal,
    int StoryPoints,
    bool IsSubtask,
    bool AwaitingApproval);

public sealed record BoardColumnDto(WorkTaskStatus Status, string Title, int Count, IReadOnlyList<TaskCardDto> Cards);

public sealed record BoardFiltersDto(
    Guid? ProjectId,
    bool LooseOnly,
    Guid? AssigneeId,
    string? Query,
    IReadOnlyList<ProjectOption> Projects,
    IReadOnlyList<PersonChip> People,
    bool HasActiveSprint);

public sealed record BoardDto(
    IReadOnlyList<BoardColumnDto> Columns,
    BoardFiltersDto Filters,
    int TotalTasks,
    string Subtitle,
    Guid? OpenTaskId);

public sealed record TaskDependencyOption(Guid Id, string Label, bool Selected);

public sealed record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid? ProjectId,
    string? ProjectCode,
    string? ProjectName,
    Guid? ParentId,
    string? ParentTitle,
    WorkTaskStatus Status,
    string StatusLabel,
    TaskPriority Priority,
    string PriorityLabel,
    string? Tag,
    PersonChip? Owner,
    IReadOnlyList<PersonChip> AssignedTeam,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal EstimateHours,
    int StoryPoints,
    TaskProgressMode ProgressMode,
    TimeSpan Logged,
    int ProgressPct,
    int ChecklistDone,
    int ChecklistTotal,
    IReadOnlyList<ChecklistItemDto> Checklist,
    IReadOnlyList<SubtaskLine> Subtasks,
    decimal UnallocatedHours,
    bool Blocked,
    IReadOnlyList<string> BlockedBy,
    string? BlockerMessage,
    bool TimerRunning,
    DateTime? TimerStartedAt,
    bool AwaitingApproval,
    bool CanChangeProject,
    bool CanChangeProgressMode,
    IReadOnlyList<TaskDependencyOption> DependencyOptions,
    DateTime CreatedAt,
    bool IsSubtask);

public sealed record SubtaskInput(string Title, decimal Hours, Guid? AssigneeId, DateOnly? DueDate);

public sealed class TaskFormModel
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? AssigneeId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public string? Tag { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? StartDate { get; set; }
    public decimal EstimateHours { get; set; } = 4m;
    public int StoryPoints { get; set; } = 3;
    public bool InBacklog { get; set; } = true;
    public TaskProgressMode ProgressMode { get; set; } = TaskProgressMode.Hours;
    public string? Description { get; set; }
    public List<SubtaskInput> Subtasks { get; set; } = [];
    public List<Guid> DependsOn { get; set; } = [];
}

public sealed record TaskFormOptionsDto(
    IReadOnlyList<ProjectOption> Projects,
    IReadOnlyList<PersonChip> People,
    IReadOnlyList<TaskDependencyOption> DependencyOptions,
    TaskFormModel Defaults);

public sealed record MoveTaskResult(bool Success, string? Error, TaskCardDto? Card, TaskDetailDto? Detail, string? Toast);

public sealed record AvailabilityWarning(string Message);

public sealed record SearchHitDto(string Title, string Type, string Url, string? Context);
