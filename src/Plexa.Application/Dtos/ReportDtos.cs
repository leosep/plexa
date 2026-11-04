using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record SelectOption(string Value, string Label, bool Selected = false);

public sealed record ReportFilters(
    string? Sprint,
    Guid? MemberId,
    string? Tag,
    bool TimeLogged,
    bool VarianceGt0)
{
    public static ReportFilters Empty { get; } = new(null, null, null, false, false);
}

public sealed record HoursRowDto(
    Guid TaskId,
    string Task,
    PersonChip? Owner,
    string? Tag,
    decimal TaskHours,
    TimeSpan Logged,
    TimeSpan Variance,
    bool HasVariance,
    bool Overdue);

public sealed record HoursReportDto(
    ReportFilters Filters,
    IReadOnlyList<SelectOption> SprintOptions,
    IReadOnlyList<PersonChip> People,
    IReadOnlyList<HoursRowDto> Rows,
    decimal TotalTaskHours,
    TimeSpan TotalLogged,
    TimeSpan TotalVariance,
    string FooterNote);

public sealed record DepartmentRowDto(
    string Department,
    string? LeadName,
    int ActiveProjects,
    int Members,
    int CompletedTasks,
    int AwaitingApproval,
    decimal Hours);

public sealed record ReportPageDto(HoursReportDto Hours, IReadOnlyList<DepartmentRowDto> Departments);
