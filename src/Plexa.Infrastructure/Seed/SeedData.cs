using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plexa.Application.Abstractions;
using Plexa.Domain.Entities;
using Plexa.Domain.Enums;
using Plexa.Infrastructure.Identity;

namespace Plexa.Infrastructure.Seed;

/// <summary>Datos semilla que reproducen el espacio documentado en la spec. Idempotente.</summary>
public static class SeedData
{
    public const string DemoPassword = "Plexa123$";

    public static async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<PlexaDbContext>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var clock = sp.GetRequiredService<IClock>();
        var storage = sp.GetRequiredService<IFileStorage>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

        if (await db.Workspaces.AnyAsync(ct))
        {
            logger.LogInformation("Seed skipped: the workspace already exists.");
            return;
        }

        await db.Database.MigrateAsync(ct);

        foreach (var roleName in new[] { "Admin", "Member" })
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                await roles.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }
        }

        var workspace = new Workspace
        {
            Name = "Grupo Dominican Watchman",
            Plan = WorkspacePlan.Free,
            DefaultLocale = "en"
        };
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(ct);

        var gerson = await EnsureUserAsync(users, workspace.Id, "gerson.cabreja@dwn.com.do", "Gerson Cabreja", "Administrator", WorkspaceRole.Admin, clock);
        var leandro = await EnsureUserAsync(users, workspace.Id, "lsepulveda@dwn.com.do", "Leandro Sepulveda", "Full-Stack Developer", WorkspaceRole.Member, clock);
        var yoderick = await EnsureUserAsync(users, workspace.Id, "ymontero@dwn.com.do", "Yoderick Emmanuel Montero Mejia", "Full-Stack Developer", WorkspaceRole.Member, clock);

        var department = new Department
        {
            WorkspaceId = workspace.Id,
            Name = "Software Development",
            LeadUserId = gerson.Id
        };
        db.Departments.Add(department);
        await db.SaveChangesAsync(ct);

        var today = clock.Today;
        var project = new Project
        {
            WorkspaceId = workspace.Id,
            Code = "PRY-01",
            Name = "Sistema de Gestión de Capacitaciones Internas",
            Client = "Internal development",
            LeadUserId = leandro.Id,
            DepartmentId = department.Id,
            Status = ProjectStatus.AtRisk,
            Health = ProjectHealth.AtRisk,
            StartDate = today.AddDays(-29),
            EndDate = today.AddDays(31),
            Description = "Internal training management system for HR."
        };
        db.Projects.Add(project);
        db.WorkspaceCounters.Add(new WorkspaceCounter { WorkspaceId = workspace.Id, Key = "project", Value = 1 });
        await db.SaveChangesAsync(ct);

        db.ProjectCollaborators.AddRange(
            new ProjectCollaborator { ProjectId = project.Id, UserId = gerson.Id },
            new ProjectCollaborator { ProjectId = project.Id, UserId = yoderick.Id });
        await db.SaveChangesAsync(ct);

        // 3 tareas padre + 11 subtareas = 14 nodos · 42 h estimadas.
        var completedAt = today.AddDays(-1).ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var analysis = Task(workspace.Id, project.Id, "Análisis y diseño", WorkTaskStatus.Completed, null, 0m, 3,
            today.AddDays(-29), today.AddDays(-20), null);
        analysis.CompletedAt = completedAt;
        analysis.ApprovedAt = completedAt;
        analysis.ApprovedByUserId = gerson.Id;
        analysis.CreatedAt = today.AddDays(-29).ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        var backend1 = Task(workspace.Id, project.Id, "Backend núcleo [1]", WorkTaskStatus.Review, null, 0m, 3,
            today.AddDays(-25), today.AddDays(6), null, inBacklog: false);
        backend1.CreatedAt = today.AddDays(-25).ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        var backend2 = Task(workspace.Id, project.Id, "Backend núcleo [2]", WorkTaskStatus.ToDo, leandro.Id, 0m, 3,
            today.AddDays(-5), today.AddDays(17), TaskPriority.High);
        backend2.CreatedAt = today.AddDays(-5).ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        db.Tasks.AddRange(analysis, backend1, backend2);
        await db.SaveChangesAsync(ct);

        var modelo = Subtask(workspace.Id, project.Id, analysis.Id, "Modelo de datos, reglas y contrato de API", 4m, leandro.Id,
            WorkTaskStatus.Completed, today.AddDays(-20), analysis.InBacklog);
        modelo.CompletedAt = completedAt;
        modelo.ApprovedAt = completedAt;
        modelo.ApprovedByUserId = gerson.Id;
        modelo.CreatedAt = today.AddDays(-29).ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        var wireframes = Subtask(workspace.Id, project.Id, analysis.Id, "Wireframes y validación con RRHH", 3m, leandro.Id,
            WorkTaskStatus.Completed, today.AddDays(-20), analysis.InBacklog);
        wireframes.CompletedAt = completedAt;
        wireframes.ApprovedAt = completedAt;
        wireframes.ApprovedByUserId = gerson.Id;
        wireframes.CreatedAt = today.AddDays(-29).ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        // Las subtareas pendientes siguen en el backlog aunque su padre esté en Review.
        var subtasks1 = new[]
        {
            Subtask(workspace.Id, project.Id, backend1.Id, "Setup y Clean Architecture", 3m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(-22), true, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend1.Id, "Seguridad administrativa", 4m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(-18), true, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend1.Id, "Acceso de empleado", 3m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(-10), true, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend1.Id, "Empleados y carga masiva", 3m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(-4), true, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend1.Id, "Catálogos", 4m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(3), true, TaskPriority.High)
        };

        var subtasks2 = new[]
        {
            Subtask(workspace.Id, project.Id, backend2.Id, "Capacitaciones y sesiones", 4m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(14), backend2.InBacklog, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend2.Id, "API de cartelera", 2m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(10), backend2.InBacklog, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend2.Id, "Inscripción y cancelación", 6m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(20), backend2.InBacklog, TaskPriority.High),
            Subtask(workspace.Id, project.Id, backend2.Id, "Asistencia en sitio", 6m, leandro.Id, WorkTaskStatus.ToDo, today.AddDays(24), backend2.InBacklog, TaskPriority.High)
        };

        db.Tasks.AddRange(modelo, wireframes);
        db.Tasks.AddRange(subtasks1);
        db.Tasks.AddRange(subtasks2);
        await db.SaveChangesAsync(ct);

        db.TaskDependencies.Add(new TaskDependency { TaskId = backend2.Id, DependsOnTaskId = backend1.Id });
        await db.SaveChangesAsync(ct);

        // 1 minuto registrado en "Análisis y diseño".
        db.TimeEntries.Add(new TimeEntry
        {
            TaskId = analysis.Id,
            UserId = leandro.Id,
            StartedAt = clock.UtcNow.AddDays(-1).AddMinutes(-2),
            EndedAt = clock.UtcNow.AddDays(-1).AddMinutes(-1),
            Seconds = 60
        });
        await db.SaveChangesAsync(ct);

        // Documento de 4.8 MB.
        await SeedDocumentAsync(db, storage, workspace.Id, project.Id, gerson.Id, logger, ct);

        // Evento del 09 Oct (hace 21 días), 15:00–16:00.
        db.CalendarEvents.Add(new CalendarEvent
        {
            WorkspaceId = workspace.Id,
            Title = "sprint-revision_proyectos",
            Date = today.AddDays(-21),
            StartsAt = new TimeOnly(15, 0),
            EndsAt = new TimeOnly(16, 0),
            Kind = CalendarEventKind.Meeting,
            ProjectId = project.Id,
            CreatedByUserId = gerson.Id
        });
        await db.SaveChangesAsync(ct);

        SeedActivity(db, workspace.Id, project.Id, gerson.Id, leandro.Id, clock, analysis, modelo, wireframes, backend1);
        SeedNotifications(db, workspace.Id, project.Id, leandro.Id, gerson.Id, clock, backend1);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seed completed. Demo password: {Password}", DemoPassword);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> users,
        Guid workspaceId,
        string email,
        string fullName,
        string jobTitle,
        WorkspaceRole role,
        IClock clock)
    {
        var existing = await users.FindByEmailAsync(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            JobTitle = jobTitle,
            Role = role,
            WorkspaceId = workspaceId,
            CapacityHoursPerWeek = 40m,
            CreatedAt = clock.UtcNow
        };

        var result = await users.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Could not create {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await users.AddToRoleAsync(user, role == WorkspaceRole.Admin ? "Admin" : "Member");
        return user;
    }

    private static WorkTask Task(
        Guid workspaceId,
        Guid projectId,
        string title,
        WorkTaskStatus status,
        Guid? assigneeId,
        decimal hours,
        int points,
        DateOnly start,
        DateOnly due,
        TaskPriority? priority,
        bool inBacklog = true) =>
        new()
        {
            WorkspaceId = workspaceId,
            ProjectId = projectId,
            Title = title,
            Status = status,
            AssigneeId = assigneeId,
            EstimateHours = hours,
            StoryPoints = points,
            StartDate = start,
            DueDate = due,
            Priority = priority ?? TaskPriority.Medium,
            InBacklog = inBacklog,
            ProgressMode = TaskProgressMode.Hours,
            CreatedAt = start.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
        };

    private static WorkTask Subtask(
        Guid workspaceId,
        Guid projectId,
        Guid parentId,
        string title,
        decimal hours,
        Guid? assigneeId,
        WorkTaskStatus status,
        DateOnly due,
        bool inBacklog,
        TaskPriority priority = TaskPriority.Medium) =>
        new()
        {
            WorkspaceId = workspaceId,
            ProjectId = projectId,
            ParentId = parentId,
            Title = title,
            EstimateHours = hours,
            AssigneeId = assigneeId,
            Status = status,
            DueDate = due,
            InBacklog = inBacklog,
            Priority = priority,
            ProgressMode = TaskProgressMode.Hours
        };

    private static async Task SeedDocumentAsync(
        PlexaDbContext db,
        IFileStorage storage,
        Guid workspaceId,
        Guid projectId,
        Guid uploadedBy,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            var pdf = BuildPlaceholderPdf();
            await using var stream = new MemoryStream(pdf);
            var key = await storage.SaveAsync(stream, "SGCI_Presentacion_Wireframes.pdf", ct);
            db.Documents.Add(new Document
            {
                WorkspaceId = workspaceId,
                ProjectId = projectId,
                FileName = "SGCI_Presentacion_Wireframes.pdf",
                ContentType = "application/pdf",
                SizeBytes = pdf.Length,
                UploadedByUserId = uploadedBy,
                StorageKey = key,
                CreatedAt = DateTime.UtcNow.AddDays(-16)
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not seed the project document.");
        }
    }

    /// <summary>PDF mínimo válido rellenado hasta ~4.8 MB para reproducir la captura.</summary>
    private static byte[] BuildPlaceholderPdf()
    {
        var body = new StringBuilder();
        body.Append("%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n");
        body.Append("2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n");
        body.Append("3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 595 842]/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>>endobj\n");
        body.Append("4 0 obj<</Length 90>>stream\nBT /F1 24 Tf 72 760 Td (SGCI - Wireframes) Tj ET\nendstream endobj\n");
        body.Append("5 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\n");
        var prefix = Encoding.ASCII.GetBytes(body.ToString());

        var target = (int)(4.8 * 1024 * 1024);
        var padding = new byte[Math.Max(0, target - prefix.Length - 64)];
        padding.AsSpan().Fill((byte)' ');
        var trailer = Encoding.ASCII.GetBytes("\ntrailer<</Root 1 0 R>>\n%%EOF\n");

        var result = new byte[prefix.Length + padding.Length + trailer.Length];
        prefix.CopyTo(result, 0);
        padding.CopyTo(result, prefix.Length);
        trailer.CopyTo(result, prefix.Length + padding.Length);
        return result;
    }

    private static void SeedActivity(
        PlexaDbContext db,
        Guid workspaceId,
        Guid projectId,
        Guid gersonId,
        Guid leandroId,
        IClock clock,
        WorkTask analysis,
        WorkTask modelo,
        WorkTask wireframes,
        WorkTask backend1)
    {
        var entries = new List<(int DaysAgo, Guid Actor, ActivityVerb Verb, string Text, Guid? TaskId)>
        {
            (2, gersonId, ActivityVerb.Moved, "Task \"Backend núcleo [1]\" moved to In review", backend1.Id),
            (2, gersonId, ActivityVerb.MarkedDone, "Task \"Análisis y diseño\" marked done", analysis.Id),
            (2, gersonId, ActivityVerb.MarkedDone, "Task \"Wireframes y validación con RRHH\" marked done", wireframes.Id),
            (2, gersonId, ActivityVerb.MarkedDone, "Task \"Modelo de datos, reglas y contrato de API\" marked done", modelo.Id),
            (3, leandroId, ActivityVerb.Moved, "Task \"Análisis y diseño\" moved to In review", analysis.Id),
            (5, leandroId, ActivityVerb.Created, "Task \"Backend núcleo [2]\" created", null),
            (6, gersonId, ActivityVerb.Created, "Task \"CRUD de Usuarios y Sucursales\" created", null),
            (7, leandroId, ActivityVerb.Moved, "Task \"Backend núcleo [1]\" moved to In progress", backend1.Id),
            (9, gersonId, ActivityVerb.Assigned, "Task \"Backend núcleo [1]\" assigned", backend1.Id),
            (11, leandroId, ActivityVerb.Created, "Subtask \"Catálogos\" created", null),
            (13, gersonId, ActivityVerb.Moved, "Task \"Análisis y diseño\" moved to In progress", analysis.Id),
            (15, leandroId, ActivityVerb.Created, "Subtask \"API de cartelera\" created", null),
            (17, gersonId, ActivityVerb.Created, "Task \"Análisis y diseño\" created", analysis.Id),
            (19, leandroId, ActivityVerb.Moved, "Task \"Wireframes y validación con RRHH\" moved to Review", wireframes.Id),
            (21, gersonId, ActivityVerb.Created, "Task \"Backend núcleo [1]\" created", backend1.Id),
            (23, leandroId, ActivityVerb.Commented, "Comment added to \"Modelo de datos, reglas y contrato de API\"", modelo.Id),
            (24, gersonId, ActivityVerb.DocumentUploaded, "Document \"SGCI_Presentacion_Wireframes.pdf\" uploaded", null),
            (26, leandroId, ActivityVerb.Created, "Subtask \"Modelo de datos, reglas y contrato de API\" created", null),
            (27, gersonId, ActivityVerb.Moved, "Task \"Modelo de datos, reglas y contrato de API\" moved to In progress", modelo.Id),
            (1, gersonId, ActivityVerb.Moved, "Task \"Backend núcleo [1]\" moved to In review", backend1.Id)
        };

        foreach (var entry in entries)
        {
            db.ActivityLogs.Add(new ActivityLog
            {
                WorkspaceId = workspaceId,
                ProjectId = projectId,
                ActorId = entry.Actor,
                Verb = entry.Verb,
                TaskId = entry.TaskId,
                Text = entry.Text,
                CreatedAt = clock.UtcNow.AddDays(-entry.DaysAgo).AddHours(-2)
            });
        }
    }

    private static void SeedNotifications(
        PlexaDbContext db,
        Guid workspaceId,
        Guid projectId,
        Guid leandroId,
        Guid gersonId,
        IClock clock,
        WorkTask backend1)
    {
        var texts = new[]
        {
            "Task \"Backend núcleo [1]\" moved to In review",
            "Task \"Análisis y diseño\" marked done",
            "Task \"Wireframes y validación con RRHH\" marked done",
            "Task \"Modelo de datos, reglas y contrato de API\" marked done"
        };

        for (var i = 0; i < texts.Length; i++)
        {
            db.Notifications.Add(new Notification
            {
                WorkspaceId = workspaceId,
                UserId = leandroId,
                Type = NotificationType.TaskMoved,
                Text = texts[i],
                ActorId = gersonId,
                TaskId = backend1.Id,
                ProjectId = projectId,
                CreatedAt = clock.UtcNow.AddDays(-2).AddMinutes(i * 15)
            });
        }
    }
}
