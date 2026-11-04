using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Plexa.Domain.Entities;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure;

public class PlexaDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PlexaDbContext(DbContextOptions<PlexaDbContext> options) : base(options)
    {
    }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceCounter> WorkspaceCounters => Set<WorkspaceCounter>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectCollaborator> ProjectCollaborators => Set<ProjectCollaborator>();
    public DbSet<Phase> Phases => Set<Phase>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();
    public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
    public DbSet<ChecklistStep> ChecklistSteps => Set<ChecklistStep>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<CalendarEventMember> CalendarEventMembers => Set<CalendarEventMember>();
    public DbSet<CalendarEventDepartment> CalendarEventDepartments => Set<CalendarEventDepartment>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ChatThread> ChatThreads => Set<ChatThread>();
    public DbSet<ThreadMember> ThreadMembers => Set<ThreadMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<UserMailbox> UserMailboxes => Set<UserMailbox>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(PlexaDbContext).Assembly);
        ApplyDefaults(builder);
    }

    private static void ApplyDefaults(ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entity.ClrType))
            {
                builder.Entity(entity.ClrType).Property(nameof(AuditableEntity.Id))
                    .HasDefaultValueSql("NEWSEQUENTIALID()");
            }
        }
    }

    public override int SaveChanges()
    {
        TouchTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void TouchTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = entry.Entity.CreatedAt == default ? now : entry.Entity.CreatedAt;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
