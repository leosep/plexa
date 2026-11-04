using Plexa.Domain.Enums;

namespace Plexa.Domain.Entities;

/// <summary>Base con identidad y marcas de auditoría (UTC).</summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Entidad que pertenece a un espacio de trabajo.</summary>
public abstract class WorkspaceEntity : AuditableEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
}

public class Workspace : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public WorkspacePlan Plan { get; set; } = WorkspacePlan.Free;
    public string? DefaultLocale { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

/// <summary>Código correlativo por espacio (PRY-01) con concurrencia optimista.</summary>
public class WorkspaceCounter : WorkspaceEntity
{
    public string Key { get; set; } = string.Empty;
    public int Value { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class Invitation : WorkspaceEntity
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public Guid? AcceptedByUserId { get; set; }
}

public class Department : WorkspaceEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? LeadUserId { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class Project : WorkspaceEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? LeadUserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public ProjectHealth Health { get; set; } = ProjectHealth.OnTrack;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public Department? Department { get; set; }
    public ICollection<ProjectCollaborator> Collaborators { get; set; } = new List<ProjectCollaborator>();
    public ICollection<Phase> Phases { get; set; } = new List<Phase>();
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}

public class ProjectCollaborator : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid UserId { get; set; }
}

public class Phase : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Position { get; set; }
}

public class Milestone : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public bool Done { get; set; }
}

public class Sprint : WorkspaceEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int CapacityPoints { get; set; }
    public int CommittedPoints { get; set; }
    public SprintStatus Status { get; set; } = SprintStatus.Planned;
}

public class WorkTask : WorkspaceEntity
{
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid? ParentId { get; set; }
    public WorkTask? Parent { get; set; }
    public ICollection<WorkTask> Subtasks { get; set; } = new List<WorkTask>();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public string? Tag { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal EstimateHours { get; set; } = 4m;
    public int StoryPoints { get; set; } = 3;
    public TaskProgressMode ProgressMode { get; set; } = TaskProgressMode.Hours;
    public bool InBacklog { get; set; } = true;
    public Guid? SprintId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public int ReopenedCount { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
    public ICollection<TaskDependency> Dependents { get; set; } = new List<TaskDependency>();
    public ICollection<ChecklistStep> Checklist { get; set; } = new List<ChecklistStep>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public bool IsSubtask => ParentId.HasValue;
}

public class TaskDependency : AuditableEntity
{
    public Guid TaskId { get; set; }
    public WorkTask? Task { get; set; }
    public Guid DependsOnTaskId { get; set; }
    public WorkTask? DependsOnTask { get; set; }
}

public class ChecklistStep : AuditableEntity
{
    public Guid TaskId { get; set; }
    public WorkTask? Task { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Done { get; set; }
    public int Position { get; set; }
}

public class TimeEntry : AuditableEntity
{
    public Guid TaskId { get; set; }
    public WorkTask? Task { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int Seconds { get; set; }
    public string? Note { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public bool IsRunning => EndedAt is null;
}

public class CalendarEvent : WorkspaceEntity
{
    public string Title { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
    public CalendarEventKind Kind { get; set; } = CalendarEventKind.Meeting;
    public string? Place { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public ICollection<CalendarEventMember> Members { get; set; } = new List<CalendarEventMember>();
    public ICollection<CalendarEventDepartment> Departments { get; set; } = new List<CalendarEventDepartment>();
}

public class CalendarEventMember : AuditableEntity
{
    public Guid CalendarEventId { get; set; }
    public CalendarEvent? CalendarEvent { get; set; }
    public Guid UserId { get; set; }
}

public class CalendarEventDepartment : AuditableEntity
{
    public Guid CalendarEventId { get; set; }
    public CalendarEvent? CalendarEvent { get; set; }
    public Guid DepartmentId { get; set; }
}

public class Document : WorkspaceEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long SizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string StorageKey { get; set; } = string.Empty;
}

public class ChatThread : WorkspaceEntity
{
    public ThreadScope Scope { get; set; } = ThreadScope.Project;
    public Guid? ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ThreadMember> Members { get; set; } = new List<ThreadMember>();
}

public class ThreadMember : AuditableEntity
{
    public Guid ThreadId { get; set; }
    public ChatThread? Thread { get; set; }
    public Guid UserId { get; set; }
    public DateTime? LastReadAt { get; set; }
}

public class Message : AuditableEntity
{
    public Guid ThreadId { get; set; }
    public ChatThread? Thread { get; set; }
    public Guid AuthorId { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? Mentions { get; set; }
}

public class Notification : WorkspaceEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid? ActorId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ThreadId { get; set; }
    public DateTime? ReadAt { get; set; }

    public bool IsRead => ReadAt.HasValue;
}

public class ActivityLog : WorkspaceEntity
{
    public Guid? ProjectId { get; set; }
    public Guid ActorId { get; set; }
    public ActivityVerb Verb { get; set; }
    public Guid? TaskId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class UserMailbox : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string? ProtectedPassword { get; set; }
}
