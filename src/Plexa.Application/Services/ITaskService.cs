using Plexa.Application.Dtos;
using Plexa.Domain.Enums;

namespace Plexa.Application.Services;

public interface ITaskService
{
    Task<BoardDto> GetBoardAsync(BoardQuery query, CancellationToken ct = default);
    Task<TaskDetailDto?> GetDetailAsync(Guid taskId, CancellationToken ct = default);
    Task<TaskCardDto?> GetCardAsync(Guid taskId, CancellationToken ct = default);
    Task<TaskFormOptionsDto> GetFormOptionsAsync(Guid? taskId = null, Guid? projectId = null, CancellationToken ct = default);
    Task<TaskFormModel?> GetFormAsync(Guid taskId, CancellationToken ct = default);
    Task<(bool Ok, string? Error, Guid TaskId)> CreateAsync(TaskFormModel model, Guid actorId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateAsync(TaskFormModel model, Guid actorId, CancellationToken ct = default);
    Task<MoveTaskResult> MoveAsync(Guid taskId, WorkTaskStatus status, Guid actorId, CancellationToken ct = default);
    Task<MoveTaskResult> ApproveAsync(Guid taskId, Guid actorId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteAsync(Guid taskId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> AddSubtaskAsync(Guid parentId, string title, decimal hours, Guid? assigneeId, Guid actorId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> AddChecklistStepAsync(Guid taskId, string text, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> ToggleChecklistStepAsync(Guid stepId, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteChecklistStepAsync(Guid stepId, CancellationToken ct = default);
    Task<AvailabilityWarning?> CheckAvailabilityAsync(Guid? assigneeId, DateOnly? start, DateOnly? due, decimal hours, Guid? excludeTaskId, CancellationToken ct = default);
    Task<IReadOnlyList<SearchHitDto>> SearchAsync(string query, CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
}

public sealed record BoardQuery(
    Guid? ProjectId = null,
    bool LooseOnly = false,
    Guid? AssigneeId = null,
    string? Q = null,
    Guid? OpenTaskId = null);

public interface ITimerService
{
    Task<TimerDto?> GetActiveAsync(Guid userId, CancellationToken ct = default);
    Task<TimerDto> StartAsync(Guid taskId, Guid userId, CancellationToken ct = default);
    Task<TimerDto?> StopAsync(Guid userId, CancellationToken ct = default);
    Task<TodayTrackedDto> GetTodayAsync(Guid userId, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(Guid userId, CancellationToken ct = default);
}

public interface ICapacityService
{
    Task<IReadOnlyList<MemberCapacityDto>> GetTeamAsync(CancellationToken ct = default);
}
