using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Enums;

namespace Plexa.Infrastructure.Services;

/// <summary>Datos del espacio que necesita el chrome de la aplicación (barra lateral).</summary>
public sealed class WorkspaceService : ServiceBase, IWorkspaceService
{
    public WorkspaceService(PlexaDbContext db, IUserContext user, IClock clock) : base(db, user, clock)
    {
    }

    public async Task<WorkspaceSidebarDto> GetSidebarAsync(CancellationToken ct = default)
    {
        var name = await Db.Workspaces.AsNoTracking()
            .Where(w => w.Id == WorkspaceId)
            .Select(w => w.Name)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var sprint = await Db.Sprints.AsNoTracking()
            .Where(s => s.WorkspaceId == WorkspaceId && s.Status == SprintStatus.Active)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(ct);

        if (sprint is null)
        {
            return new WorkspaceSidebarDto(name, null, null, 0, 0, 0);
        }

        var range = $"{sprint.StartDate.ToString("dd MMM", CultureInfo.CurrentCulture)} – {sprint.EndDate.ToString("dd MMM", CultureInfo.CurrentCulture)}";
        var daysLeft = Math.Max(0, sprint.EndDate.DayNumber - Clock.Today.DayNumber);
        return new WorkspaceSidebarDto(name, sprint.Name, range, sprint.CommittedPoints, sprint.CapacityPoints, daysLeft);
    }
}
