using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plexa.Domain.Entities;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(x => x.FullName).HasMaxLength(160).IsRequired();
        b.Property(x => x.JobTitle).HasMaxLength(120);
        b.Property(x => x.AvatarPath).HasMaxLength(300);
        b.Property(x => x.Locale).HasMaxLength(10);
        b.Property(x => x.CapacityHoursPerWeek).HasPrecision(9, 2);
        b.HasIndex(x => x.WorkspaceId);
        b.HasIndex(x => x.FullName);
    }
}

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> b)
    {
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.LogoPath).HasMaxLength(300);
        b.Property(x => x.DefaultLocale).HasMaxLength(10);
    }
}

public class WorkspaceCounterConfiguration : IEntityTypeConfiguration<WorkspaceCounter>
{
    public void Configure(EntityTypeBuilder<WorkspaceCounter> b)
    {
        b.Property(x => x.Key).HasMaxLength(60).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.WorkspaceId, x.Key }).IsUnique();
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> b)
    {
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.Code).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.HasIndex(x => new { x.WorkspaceId, x.Name }).IsUnique();
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Client).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Budget).HasPrecision(18, 2);
        b.Property(x => x.StartDate).HasColumnType("date");
        b.Property(x => x.EndDate).HasColumnType("date");
        b.HasIndex(x => new { x.WorkspaceId, x.Code }).IsUnique();
        b.HasIndex(x => x.Status);
        b.HasOne(x => x.Workspace).WithMany(w => w.Projects).HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Department).WithMany(d => d.Projects).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProjectCollaboratorConfiguration : IEntityTypeConfiguration<ProjectCollaborator>
{
    public void Configure(EntityTypeBuilder<ProjectCollaborator> b)
    {
        b.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
        b.HasOne(x => x.Project).WithMany(p => p.Collaborators).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> b)
    {
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.StartDate).HasColumnType("date");
        b.Property(x => x.EndDate).HasColumnType("date");
        b.HasOne(x => x.Project).WithMany(p => p.Phases).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.DueDate).HasColumnType("date");
        b.HasOne(x => x.Project).WithMany(p => p.Milestones).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> b)
    {
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.StartDate).HasColumnType("date");
        b.Property(x => x.EndDate).HasColumnType("date");
        b.HasIndex(x => new { x.WorkspaceId, x.Status });
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Tag).HasMaxLength(40);
        b.Property(x => x.EstimateHours).HasPrecision(9, 2);
        b.Property(x => x.StartDate).HasColumnType("date");
        b.Property(x => x.DueDate).HasColumnType("date");
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasIndex(x => new { x.ProjectId, x.Status });
        b.HasIndex(x => new { x.AssigneeId, x.DueDate });
        b.HasIndex(x => x.ParentId);
        b.HasIndex(x => x.WorkspaceId);

        b.ToTable(t => t.HasCheckConstraint("CK_Task_Dates",
            "[StartDate] IS NULL OR [DueDate] IS NULL OR [StartDate] <= [DueDate]"));

        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Project).WithMany(p => p.Tasks).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Parent).WithMany(p => p.Subtasks).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    public void Configure(EntityTypeBuilder<TaskDependency> b)
    {
        b.HasIndex(x => new { x.TaskId, x.DependsOnTaskId }).IsUnique();
        b.HasOne(x => x.Task).WithMany(t => t.Dependencies).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DependsOnTask).WithMany(t => t.Dependents).HasForeignKey(x => x.DependsOnTaskId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChecklistStepConfiguration : IEntityTypeConfiguration<ChecklistStep>
{
    public void Configure(EntityTypeBuilder<ChecklistStep> b)
    {
        b.Property(x => x.Text).HasMaxLength(300).IsRequired();
        b.HasOne(x => x.Task).WithMany(t => t.Checklist).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> b)
    {
        b.Property(x => x.Note).HasMaxLength(400);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.UserId, x.StartedAt });
        b.HasIndex(x => x.TaskId);
        b.HasIndex(x => x.UserId).IsUnique().HasFilter("[EndedAt] IS NULL");
        b.HasOne(x => x.Task).WithMany(t => t.TimeEntries).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> b)
    {
        b.Property(x => x.Title).HasMaxLength(160).IsRequired();
        b.Property(x => x.Place).HasMaxLength(160);
        b.Property(x => x.Date).HasColumnType("date");
        b.Property(x => x.StartsAt).HasColumnType("time");
        b.Property(x => x.EndsAt).HasColumnType("time");
        b.HasIndex(x => new { x.WorkspaceId, x.Date });
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class CalendarEventMemberConfiguration : IEntityTypeConfiguration<CalendarEventMember>
{
    public void Configure(EntityTypeBuilder<CalendarEventMember> b)
    {
        b.HasIndex(x => new { x.CalendarEventId, x.UserId }).IsUnique();
        b.HasOne(x => x.CalendarEvent).WithMany(e => e.Members).HasForeignKey(x => x.CalendarEventId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CalendarEventDepartmentConfiguration : IEntityTypeConfiguration<CalendarEventDepartment>
{
    public void Configure(EntityTypeBuilder<CalendarEventDepartment> b)
    {
        b.HasIndex(x => new { x.CalendarEventId, x.DepartmentId }).IsUnique();
        b.HasOne(x => x.CalendarEvent).WithMany(e => e.Departments).HasForeignKey(x => x.CalendarEventId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(160).IsRequired();
        b.Property(x => x.StorageKey).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.StorageKey).IsUnique();
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Project).WithMany(p => p.Documents).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatThreadConfiguration : IEntityTypeConfiguration<ChatThread>
{
    public void Configure(EntityTypeBuilder<ChatThread> b)
    {
        b.Property(x => x.Title).HasMaxLength(160).IsRequired();
        b.HasIndex(x => new { x.WorkspaceId, x.Scope });
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ThreadMemberConfiguration : IEntityTypeConfiguration<ThreadMember>
{
    public void Configure(EntityTypeBuilder<ThreadMember> b)
    {
        b.HasIndex(x => new { x.ThreadId, x.UserId }).IsUnique();
        b.HasOne(x => x.Thread).WithMany(t => t.Members).HasForeignKey(x => x.ThreadId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Mentions).HasMaxLength(400);
        b.HasIndex(x => new { x.ThreadId, x.CreatedAt });
        b.HasOne(x => x.Thread).WithMany(t => t.Messages).HasForeignKey(x => x.ThreadId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.Property(x => x.Text).HasMaxLength(400).IsRequired();
        b.HasIndex(x => new { x.UserId, x.ReadAt });
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> b)
    {
        b.Property(x => x.Text).HasMaxLength(400).IsRequired();
        b.HasIndex(x => new { x.WorkspaceId, x.CreatedAt });
        b.HasIndex(x => x.ProjectId);
        b.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class UserMailboxConfiguration : IEntityTypeConfiguration<UserMailbox>
{
    public void Configure(EntityTypeBuilder<UserMailbox> b)
    {
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.SmtpHost).HasMaxLength(160).IsRequired();
        b.Property(x => x.ProtectedPassword).HasMaxLength(1000);
        b.HasIndex(x => x.UserId).IsUnique();
    }
}
