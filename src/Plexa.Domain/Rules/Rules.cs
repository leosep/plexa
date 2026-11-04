using Plexa.Domain.Enums;

namespace Plexa.Domain.Rules;

/// <summary>Peso de una subtarea para calcular el avance de la tarea padre.</summary>
public readonly record struct SubtaskWeight(decimal Hours, bool Completed);

/// <summary>Cálculo del % de avance de una tarea (regla 2 del dominio).</summary>
public static class ProgressCalculator
{
    public static int Calculate(TaskProgressMode mode, IReadOnlyCollection<SubtaskWeight> subtasks, bool parentCompleted)
    {
        if (subtasks.Count == 0)
        {
            return parentCompleted ? 100 : 0;
        }

        if (mode == TaskProgressMode.Percentage)
        {
            var done = subtasks.Count(s => s.Completed);
            return (int)Math.Round(done * 100d / subtasks.Count, MidpointRounding.AwayFromZero);
        }

        var total = subtasks.Sum(s => Math.Max(0m, s.Hours));
        if (total <= 0m)
        {
            var done = subtasks.Count(s => s.Completed);
            return (int)Math.Round(done * 100d / subtasks.Count, MidpointRounding.AwayFromZero);
        }

        var completedHours = subtasks.Where(s => s.Completed).Sum(s => Math.Max(0m, s.Hours));
        return (int)Math.Round(completedHours * 100m / total, MidpointRounding.AwayFromZero);
    }

    /// <summary>Horas estimadas de la tarea padre que no están repartidas en subtareas.</summary>
    public static decimal UnallocatedHours(decimal parentEstimateHours, IReadOnlyCollection<SubtaskWeight> subtasks)
    {
        var allocated = subtasks.Sum(s => Math.Max(0m, s.Hours));
        var remaining = parentEstimateHours - allocated;
        return remaining < 0m ? 0m : remaining;
    }
}

/// <summary>Progreso ponderado de un proyecto por horas estimadas (regla 10).</summary>
public static class ProjectProgressCalculator
{
    public static int Calculate(IEnumerable<(decimal Hours, bool Completed)> tasks)
    {
        var list = tasks.ToList();
        if (list.Count == 0)
        {
            return 0;
        }

        var total = list.Sum(t => Math.Max(0m, t.Hours));
        if (total <= 0m)
        {
            return (int)Math.Round(list.Count(t => t.Completed) * 100d / list.Count, MidpointRounding.AwayFromZero);
        }

        var done = list.Where(t => t.Completed).Sum(t => Math.Max(0m, t.Hours));
        return (int)Math.Round(done * 100m / total, MidpointRounding.AwayFromZero);
    }
}

/// <summary>Una dependencia pendiente mostrada en el aviso "It cannot start yet".</summary>
public readonly record struct Blocker(string TaskTitle, WorkTaskStatus Status, DateTime? ApprovedAt);

/// <summary>Regla 4: una tarea está bloqueada si alguna dependencia no está completada y aprobada.</summary>
public static class BlockingRule
{
    public static bool IsApprovedAndCompleted(WorkTaskStatus status, DateTime? approvedAt) =>
        status == WorkTaskStatus.Completed && approvedAt.HasValue;

    public static IReadOnlyList<Blocker> PendingBlockers(IEnumerable<Blocker> dependencies) =>
        dependencies.Where(d => !IsApprovedAndCompleted(d.Status, d.ApprovedAt)).ToList();

    public static bool IsBlocked(IEnumerable<Blocker> dependencies) =>
        PendingBlockers(dependencies).Count > 0;

    public static string Describe(IEnumerable<Blocker> dependencies) =>
        string.Join(" · ", PendingBlockers(dependencies).Select(d => $"{d.TaskTitle} · {StatusLabel(d.Status)}"));

    public static string StatusLabel(WorkTaskStatus status) => status switch
    {
        WorkTaskStatus.ToDo => "To do",
        WorkTaskStatus.InProgress => "In progress",
        WorkTaskStatus.Review => "Review",
        _ => "Completed"
    };
}

/// <summary>Regla 4: detección de ciclos al añadir una dependencia.</summary>
public static class DependencyCycleDetector
{
    /// <summary>
    /// Comprueba si añadir <paramref name="taskId"/> → <paramref name="dependsOnId"/> crea un ciclo.
    /// <paramref name="edges"/> contiene las dependencias actuales (task → dependsOn).
    /// </summary>
    public static bool WouldCreateCycle(Guid taskId, Guid dependsOnId, IEnumerable<(Guid Task, Guid DependsOn)> edges)
    {
        if (taskId == dependsOnId)
        {
            return true;
        }

        var adjacency = edges
            .GroupBy(e => e.Task)
            .ToDictionary(g => g.Key, g => g.Select(e => e.DependsOn).ToList());

        var stack = new Stack<Guid>();
        var visited = new HashSet<Guid>();
        stack.Push(dependsOnId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == taskId)
            {
                return true;
            }

            if (!visited.Add(current) || !adjacency.TryGetValue(current, out var next))
            {
                continue;
            }

            foreach (var node in next)
            {
                stack.Push(node);
            }
        }

        return false;
    }
}

/// <summary>Regla 11: señal de capacidad de una persona.</summary>
public static class CapacityCalculator
{
    public const decimal AtRiskThreshold = 80m;
    public const decimal AvailableThreshold = 50m;

    public static int Percent(decimal openEstimatedHours, decimal capacityHours)
    {
        if (capacityHours <= 0m)
        {
            return 0;
        }

        var pct = openEstimatedHours * 100m / capacityHours;
        return (int)Math.Round(pct, MidpointRounding.AwayFromZero);
    }

    public static CapacitySignal Signal(int capacityPercent, int overdueCount, bool hasActiveSprint)
    {
        if (!hasActiveSprint)
        {
            // Sin sprint activo no hay capacidad comprometida: la spec muestra 0 % y "On track".
            return CapacitySignal.OnTrack;
        }

        if (capacityPercent > 100)
        {
            return CapacitySignal.Overloaded;
        }

        if (capacityPercent > AtRiskThreshold || overdueCount > 0)
        {
            return CapacitySignal.AtRisk;
        }

        if (capacityPercent < AvailableThreshold)
        {
            return CapacitySignal.Available;
        }

        return CapacitySignal.OnTrack;
    }

    public static string SignalLabel(CapacitySignal signal) => signal switch
    {
        CapacitySignal.AtRisk => "At risk",
        CapacitySignal.Overloaded => "Overloaded",
        CapacitySignal.Available => "Available",
        _ => "On track"
    };

    public static string AvailabilityLabel(CapacitySignal signal) =>
        signal is CapacitySignal.Available or CapacitySignal.OnTrack ? "Available" : "Limited";
}

/// <summary>Formato de duraciones de la spec: "0h 00m", "1m", "42h 00m".</summary>
public static class DurationFormatter
{
    public static string Format(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        var hours = (int)Math.Floor(value.TotalHours);
        var minutes = value.Minutes;

        if (hours == 0 && minutes == 0)
        {
            return "0h 00m";
        }

        if (hours == 0)
        {
            return $"{minutes}m";
        }

        return $"{hours}h {minutes:00}m";
    }

    public static string Format(decimal hours) => Format(TimeSpan.FromHours((double)Math.Max(0m, hours)));

    /// <summary>Formato compacto para el pie de la tarjeta: "1m", "7h".</summary>
    public static string FormatShort(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            return "0m";
        }

        if (value.TotalHours >= 1)
        {
            return $"{(int)Math.Floor(value.TotalHours)}h";
        }

        return $"{Math.Max(1, (int)Math.Floor(value.TotalMinutes))}m";
    }
}

/// <summary>Regla 8: transiciones permitidas entre estados.</summary>
public static class TaskTransitions
{
    public static IReadOnlyList<WorkTaskStatus> Allowed(WorkTaskStatus current) =>
        Enum.GetValues<WorkTaskStatus>().Where(s => s != current).ToList();

    public static bool CanMove(WorkTaskStatus from, WorkTaskStatus to) => from != to;

    /// <summary>Mover a In progress exige que no haya dependencias pendientes.</summary>
    public static bool RequiresUnblocked(WorkTaskStatus to) => to is WorkTaskStatus.InProgress or WorkTaskStatus.Review;
}
