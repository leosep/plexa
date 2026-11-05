using Plexa.Application.Dtos;
using Plexa.Domain.Enums;

namespace Plexa.Application.Services;

public interface IProjectService
{
    Task<ProjectListDto> ListAsync(ProjectStatus? status, string? query, CancellationToken ct = default);
    Task<ProjectDetailDto?> GetDetailAsync(Guid projectId, string tab, CancellationToken ct = default);
    Task<TeamEditOptionsDto?> GetTeamEditAsync(Guid projectId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveTeamAsync(TeamEditModel model, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UploadDocumentsAsync(Guid projectId, IReadOnlyList<(string FileName, string ContentType, long Length, Stream Content)> files, Guid actorId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<(string FileName, string ContentType, Stream Content)?> OpenDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectOption>> GetVisibleProjectsAsync(CancellationToken ct = default);
    Task<bool> CanAccessAsync(Guid projectId, CancellationToken ct = default);
}

public interface IReportService
{
    Task<ReportPageDto> GetAsync(ReportFilters filters, CancellationToken ct = default);
    Task<byte[]> ExportAsync(ReportFilters filters, string format, CancellationToken ct = default);
    Task<byte[]> ExportDepartmentAsync(string format, CancellationToken ct = default);
}

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetAsync(CancellationToken ct = default);
}

public interface ICalendarService
{
    Task<CalendarDto> GetAsync(string view, DateOnly? anchor, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarEventDto>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<CalendarEventDto?> GetEventAsync(Guid id, CancellationToken ct = default);
    Task<(bool Ok, string? Error, Guid Id)> CreateAsync(CalendarEventFormModel model, Guid actorId, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarEventDto>> GetForProjectAsync(Guid projectId, CancellationToken ct = default);
}

public interface IMessageService
{
    Task<IReadOnlyList<ThreadSummaryDto>> GetThreadsAsync(CancellationToken ct = default);
    Task<ThreadDto?> GetThreadAsync(Guid threadId, CancellationToken ct = default);
    Task<(bool Ok, string? Error, Guid Id)> PostMessageAsync(Guid threadId, string body, Guid actorId, CancellationToken ct = default);
    Task<(bool Ok, string? Error, Guid Id)> CreateThreadAsync(string title, ThreadScope scope, Guid? projectId, Guid actorId, CancellationToken ct = default);
    Task<int> UnreadCountAsync(CancellationToken ct = default);
    Task MarkReadAsync(Guid threadId, CancellationToken ct = default);
}

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int limit, CancellationToken ct = default);
    Task<int> UnreadCountAsync(CancellationToken ct = default);
    Task MarkAllReadAsync(CancellationToken ct = default);
}

public interface ISettingsService
{
    Task<ProfileDto> GetProfileAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateAvatarAsync(Stream content, string fileName, string contentType, long length, CancellationToken ct = default);
    Task<MailboxDto> GetMailboxAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveMailboxAsync(string email, string smtpHost, int smtpPort, bool useStartTls, string? password, CancellationToken ct = default);
}

public interface ISearchService
{
    Task<IReadOnlyList<SearchHitDto>> SuggestAsync(string query, CancellationToken ct = default);
}

public interface IAdminService
{
    Task<IReadOnlyList<SprintDto>> GetSprintsAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> CreateSprintAsync(string name, DateOnly start, DateOnly end, int capacityPoints, int committedPoints, bool activate, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> CloseSprintAsync(Guid sprintId, CancellationToken ct = default);
    Task<IReadOnlyList<AdminMemberDto>> GetMembersAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> InviteMemberAsync(string email, WorkspaceRole role, string fullName, string jobTitle, decimal capacityHours, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateMemberAsync(Guid userId, string fullName, string jobTitle, WorkspaceRole role, decimal capacityHours, CancellationToken ct = default);
    Task<IReadOnlyList<AdminProjectDto>> GetProjectsAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error, Guid Id)> SaveProjectAsync(AdminProjectFormModel model, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> ArchiveProjectAsync(Guid projectId, CancellationToken ct = default);
}

public sealed record SprintDto(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, int CapacityPoints, int CommittedPoints, SprintStatus Status, string StatusLabel, bool IsActive);

public sealed record AdminMemberDto(Guid Id, string FullName, string Email, string? JobTitle, WorkspaceRole Role, decimal CapacityHours, string Initials);

public sealed record AdminProjectDto(Guid Id, string Code, string Name, string Client, ProjectStatus Status, ProjectHealth Health, string LeadName, DateOnly? EndDate);

public sealed class AdminProjectFormModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Client { get; set; } = "Internal development";
    public Guid? LeadUserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public ProjectHealth Health { get; set; } = ProjectHealth.OnTrack;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? Budget { get; set; }
}

/// <summary>Datos del espacio para la barra lateral (nombre y sprint activo).</summary>
public interface IWorkspaceService
{
    Task<WorkspaceSidebarDto> GetSidebarAsync(CancellationToken ct = default);
}
