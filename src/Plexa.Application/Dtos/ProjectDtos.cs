using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record ActivityItemDto(Guid Id, string Text, PersonChip Actor, DateTime At, string RelativeLabel, ActivityVerbKind Kind);

public enum ActivityVerbKind
{
    Created,
    Moved,
    Completed,
    Reopened,
    Other
}

public sealed record StatusFilterChip(ProjectStatus? Status, string Label, int Count, bool Active);

public sealed record ProjectListItemDto(
    Guid Id,
    string Code,
    string Name,
    string Client,
    string Initials,
    PersonChip? Lead,
    ProjectStatus Status,
    string StatusLabel,
    ProjectHealth Health,
    int ProgressPct,
    decimal? Budget,
    DateOnly? EndDate,
    int TasksDone,
    int TasksTotal);

public sealed record ProjectListDto(
    IReadOnlyList<KpiCard> Kpis,
    IReadOnlyList<StatusFilterChip> Chips,
    IReadOnlyList<ProjectListItemDto> Items,
    int Total,
    ProjectStatus? Status,
    string? Query);

public sealed record ProjectHeaderDto(
    Guid Id,
    string Code,
    string Name,
    string Client,
    string Initials,
    ProjectHealth Health,
    string HealthLabel,
    PersonChip? Lead,
    int ProgressPct,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int TasksDone,
    int TasksTotal,
    string MetaLine,
    bool CanManageTeam);

public sealed record PhaseDto(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, int Position);

public sealed record MilestoneDto(Guid Id, string Title, DateOnly? DueDate, bool Done);

public sealed record ProjectOverviewDto(
    IReadOnlyList<PhaseDto> Phases,
    IReadOnlyList<BoardColumnDto> Columns,
    IReadOnlyList<ActivityItemDto> Activity,
    bool ShowInsights,
    string TimelineRange);

public sealed record ProjectDashboardDto(
    IReadOnlyList<KpiCard> Kpis,
    IReadOnlyList<MilestoneDto> Milestones,
    IReadOnlyList<ActivityItemDto> Activity);

public sealed record ProjectTimelineDto(string RangeLabel, IReadOnlyList<PhaseDto> Phases);

public sealed record ProjectCalendarDto(IReadOnlyList<CalendarEventDto> Events);

public sealed record ProjectTeamDto(
    string? DepartmentName,
    PersonChip? DepartmentLead,
    PersonChip? ProjectLead,
    IReadOnlyList<PersonChip> Collaborators,
    bool CanEdit);

public sealed record ProjectDocumentDto(Guid Id, string FileName, string ContentType, long SizeBytes, PersonChip? UploadedBy, DateTime UploadedAt, string SizeLabel);

public sealed record ProjectDocumentsDto(IReadOnlyList<ProjectDocumentDto> Documents);

public sealed record ProjectDetailDto(
    ProjectHeaderDto Header,
    string ActiveTab,
    ProjectOverviewDto? Overview,
    ProjectDashboardDto? Dashboard,
    BoardDto? Board,
    ProjectTimelineDto? Timeline,
    ProjectCalendarDto? Calendar,
    ProjectTeamDto? Team,
    ProjectDocumentsDto? Documents);

public sealed record TeamEditModel
{
    public Guid ProjectId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? LeadUserId { get; set; }
    public List<Guid> CollaboratorIds { get; set; } = [];
}

public sealed record TeamEditOptionsDto(
    IReadOnlyList<(Guid Id, string Name)> Departments,
    IReadOnlyList<PersonChip> People,
    TeamEditModel Model);
