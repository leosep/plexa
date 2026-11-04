namespace Plexa.Domain.Enums;

/// <summary>Estado de una tarea en el tablero.</summary>
public enum WorkTaskStatus
{
    ToDo = 0,
    InProgress = 1,
    Review = 2,
    Completed = 3
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

/// <summary>Cómo se pondera el progreso de una tarea con subtareas.</summary>
public enum TaskProgressMode
{
    Hours = 0,
    Percentage = 1
}

public enum ProjectStatus
{
    Planning = 0,
    InProgress = 1,
    AtRisk = 2,
    OnHold = 3,
    Completed = 4,
    Archived = 5
}

public enum ProjectHealth
{
    OnTrack = 0,
    AtRisk = 1
}

public enum SprintStatus
{
    Planned = 0,
    Active = 1,
    Closed = 2
}

public enum CalendarEventKind
{
    Meeting = 0,
    Review = 1,
    Demo = 2,
    Deadline = 3,
    Retro = 4,
    Other = 5
}

public enum ThreadScope
{
    Project = 0,
    Team = 1
}

public enum WorkspaceRole
{
    Member = 0,
    Admin = 1
}

public enum WorkspacePlan
{
    Free = 0,
    Pro = 1
}

public enum NotificationType
{
    TaskCreated = 0,
    TaskMoved = 1,
    TaskCompleted = 2,
    TaskAssigned = 3,
    Mention = 4,
    Message = 5,
    Approval = 6
}

public enum ActivityVerb
{
    Created = 0,
    Moved = 1,
    MarkedDone = 2,
    Reopened = 3,
    Assigned = 4,
    Commented = 5,
    Approved = 6,
    DocumentUploaded = 7
}

public enum CapacitySignal
{
    OnTrack = 0,
    AtRisk = 1,
    Overloaded = 2,
    Available = 3
}
