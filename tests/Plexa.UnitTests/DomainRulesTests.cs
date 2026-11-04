using FluentAssertions;
using Plexa.Domain.Enums;
using Plexa.Domain.Rules;

namespace Plexa.UnitTests;

public class ProgressCalculatorTests
{
    [Fact]
    public void WithoutSubtasks_UsesParentCompletion()
    {
        ProgressCalculator.Calculate(TaskProgressMode.Hours, [], parentCompleted: false).Should().Be(0);
        ProgressCalculator.Calculate(TaskProgressMode.Hours, [], parentCompleted: true).Should().Be(100);
    }

    [Fact]
    public void PercentageMode_CountsCompletedSubtasks()
    {
        var subtasks = new[]
        {
            new SubtaskWeight(3m, true),
            new SubtaskWeight(1m, false),
            new SubtaskWeight(1m, false)
        };

        ProgressCalculator.Calculate(TaskProgressMode.Percentage, subtasks, parentCompleted: false)
            .Should().Be(33);
    }

    [Fact]
    public void HoursMode_WeightsByEstimatedHours()
    {
        var subtasks = new[]
        {
            new SubtaskWeight(6m, true),
            new SubtaskWeight(2m, false)
        };

        ProgressCalculator.Calculate(TaskProgressMode.Hours, subtasks, parentCompleted: false)
            .Should().Be(75);
    }

    [Fact]
    public void HoursMode_FallsBackToCount_WhenNoHours()
    {
        var subtasks = new[]
        {
            new SubtaskWeight(0m, true),
            new SubtaskWeight(0m, true),
            new SubtaskWeight(0m, false),
            new SubtaskWeight(0m, false)
        };

        ProgressCalculator.Calculate(TaskProgressMode.Hours, subtasks, parentCompleted: false)
            .Should().Be(50);
    }

    [Fact]
    public void NegativeHours_AreIgnored()
    {
        var subtasks = new[]
        {
            new SubtaskWeight(-5m, false),
            new SubtaskWeight(4m, true)
        };

        ProgressCalculator.Calculate(TaskProgressMode.Hours, subtasks, parentCompleted: false)
            .Should().Be(100);
    }

    [Theory]
    [InlineData(8, 3, 5)]
    [InlineData(8, 8, 0)]
    [InlineData(8, 10, 0)]
    public void UnallocatedHours_ReturnsRemainingOrZero(decimal parent, decimal allocated, decimal expected)
    {
        ProgressCalculator.UnallocatedHours(parent, [new SubtaskWeight(allocated, false)])
            .Should().Be(expected);
    }
}

public class ProjectProgressCalculatorTests
{
    [Fact]
    public void EmptyProject_ReturnsZero()
    {
        ProjectProgressCalculator.Calculate([]).Should().Be(0);
    }

    [Fact]
    public void WeightsCompletedHoursOverTotal()
    {
        var tasks = new (decimal Hours, bool Completed)[]
        {
            (10m, true),
            (6m, false),
            (4m, false)
        };

        ProjectProgressCalculator.Calculate(tasks).Should().Be(50);
    }

    [Fact]
    public void ZeroHours_FallsBackToCompletedCount()
    {
        var tasks = new (decimal Hours, bool Completed)[]
        {
            (0m, true),
            (0m, false),
            (0m, false),
            (0m, false)
        };

        ProjectProgressCalculator.Calculate(tasks).Should().Be(25);
    }

    [Fact]
    public void AllCompleted_ReturnsHundred()
    {
        var tasks = new (decimal Hours, bool Completed)[] { (2m, true), (3m, true) };
        ProjectProgressCalculator.Calculate(tasks).Should().Be(100);
    }
}

public class BlockingRuleTests
{
    private static readonly DateTime Approved = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Dependency_RequiresCompletedAndApproved()
    {
        BlockingRule.IsApprovedAndCompleted(WorkTaskStatus.Completed, Approved).Should().BeTrue();
        BlockingRule.IsApprovedAndCompleted(WorkTaskStatus.Completed, null).Should().BeFalse();
        BlockingRule.IsApprovedAndCompleted(WorkTaskStatus.Review, Approved).Should().BeFalse();
    }

    [Fact]
    public void Task_WithOnlyApprovedDependencies_IsNotBlocked()
    {
        var deps = new[]
        {
            new Blocker("Diseño", WorkTaskStatus.Completed, Approved),
            new Blocker("Modelo", WorkTaskStatus.Completed, Approved)
        };

        BlockingRule.IsBlocked(deps).Should().BeFalse();
        BlockingRule.PendingBlockers(deps).Should().BeEmpty();
    }

    [Fact]
    public void Task_WithPendingDependencies_IsBlocked()
    {
        var deps = new[]
        {
            new Blocker("Diseño", WorkTaskStatus.Completed, Approved),
            new Blocker("Backend núcleo [1]", WorkTaskStatus.Review, null),
            new Blocker("Catálogos", WorkTaskStatus.ToDo, null)
        };

        BlockingRule.IsBlocked(deps).Should().BeTrue();
        BlockingRule.PendingBlockers(deps).Should().HaveCount(2);
        BlockingRule.Describe(deps).Should().Be("Backend núcleo [1] · Review · Catálogos · To do");
    }

    [Fact]
    public void NoDependencies_IsNotBlocked()
    {
        BlockingRule.IsBlocked([]).Should().BeFalse();
        BlockingRule.Describe([]).Should().BeEmpty();
    }
}

public class DependencyCycleDetectorTests
{
    [Fact]
    public void SelfDependency_IsACycle()
    {
        var id = Guid.NewGuid();
        DependencyCycleDetector.WouldCreateCycle(id, id, []).Should().BeTrue();
    }

    [Fact]
    public void DirectCycle_IsDetected()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        // b depende de a; añadir a -> b cierra el ciclo.
        DependencyCycleDetector.WouldCreateCycle(a, b, [(b, a)]).Should().BeTrue();
    }

    [Fact]
    public void IndirectCycle_IsDetected()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        // b -> a, c -> b; añadir a -> c cierra a -> c -> b -> a.
        DependencyCycleDetector.WouldCreateCycle(a, c, [(b, a), (c, b)]).Should().BeTrue();
    }

    [Fact]
    public void IndependentEdge_IsAllowed()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        DependencyCycleDetector.WouldCreateCycle(a, b, [(c, b)]).Should().BeFalse();
    }

    [Fact]
    public void RepeatedNodes_DoNotLoopForever()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        DependencyCycleDetector.WouldCreateCycle(a, b, [(b, c), (c, b)]).Should().BeFalse();
    }
}

public class CapacityCalculatorTests
{
    [Theory]
    [InlineData(20, 40, 50)]
    [InlineData(40, 40, 100)]
    [InlineData(10, 0, 0)]
    [InlineData(0, 40, 0)]
    public void Percent_ComputesRoundedUsage(decimal open, decimal capacity, int expected)
    {
        CapacityCalculator.Percent(open, capacity).Should().Be(expected);
    }

    [Fact]
    public void Signal_WithoutSprint_IsOnTrack()
    {
        CapacityCalculator.Signal(150, 5, hasActiveSprint: false).Should().Be(CapacitySignal.OnTrack);
    }

    [Theory]
    [InlineData(101, 0, CapacitySignal.Overloaded)]
    [InlineData(81, 0, CapacitySignal.AtRisk)]
    [InlineData(50, 1, CapacitySignal.AtRisk)]
    [InlineData(40, 0, CapacitySignal.Available)]
    [InlineData(60, 0, CapacitySignal.OnTrack)]
    [InlineData(100, 0, CapacitySignal.AtRisk)]
    public void Signal_ClassifiesUsage(int percent, int overdue, CapacitySignal expected)
    {
        CapacityCalculator.Signal(percent, overdue, hasActiveSprint: true).Should().Be(expected);
    }

    [Fact]
    public void Labels_MatchSpec()
    {
        CapacityCalculator.SignalLabel(CapacitySignal.OnTrack).Should().Be("On track");
        CapacityCalculator.SignalLabel(CapacitySignal.AtRisk).Should().Be("At risk");
        CapacityCalculator.SignalLabel(CapacitySignal.Overloaded).Should().Be("Overloaded");
        CapacityCalculator.SignalLabel(CapacitySignal.Available).Should().Be("Available");

        CapacityCalculator.AvailabilityLabel(CapacitySignal.Available).Should().Be("Available");
        CapacityCalculator.AvailabilityLabel(CapacitySignal.OnTrack).Should().Be("Available");
        CapacityCalculator.AvailabilityLabel(CapacitySignal.AtRisk).Should().Be("Limited");
        CapacityCalculator.AvailabilityLabel(CapacitySignal.Overloaded).Should().Be("Limited");
    }
}

public class DurationFormatterTests
{
    [Theory]
    [InlineData(0, 0, 0, "0h 00m")]
    [InlineData(0, 1, 0, "1m")]
    [InlineData(2, 0, 0, "2h 00m")]
    [InlineData(42, 0, 0, "42h 00m")]
    [InlineData(1, 5, 0, "1h 05m")]
    [InlineData(0, 0, 30, "0h 00m")]
    public void Format_MatchesSpecExamples(int hours, int minutes, int seconds, string expected)
    {
        DurationFormatter.Format(new TimeSpan(hours, minutes, seconds)).Should().Be(expected);
    }

    [Fact]
    public void Format_NegativeClampsToZero()
    {
        DurationFormatter.Format(TimeSpan.FromMinutes(-5)).Should().Be("0h 00m");
    }

    [Fact]
    public void Format_DecimalHours_UsesSameRules()
    {
        DurationFormatter.Format(42m).Should().Be("42h 00m");
        DurationFormatter.Format(0m).Should().Be("0h 00m");
        DurationFormatter.Format(-3m).Should().Be("0h 00m");
    }

    [Theory]
    [InlineData(0, "0m")]
    [InlineData(60, "1m")]
    [InlineData(90, "1m")]
    [InlineData(3600, "1h")]
    [InlineData(7 * 3600, "7h")]
    [InlineData(7 * 3600 + 59 * 60, "7h")]
    public void FormatShort_UsesCompactUnits(int seconds, string expected)
    {
        DurationFormatter.FormatShort(TimeSpan.FromSeconds(seconds)).Should().Be(expected);
    }
}

public class TaskTransitionsTests
{
    [Fact]
    public void Allowed_ExcludesCurrentStatus()
    {
        foreach (var status in Enum.GetValues<WorkTaskStatus>())
        {
            var allowed = TaskTransitions.Allowed(status);
            allowed.Should().NotContain(status);
            allowed.Should().HaveCount(3);
        }
    }

    [Fact]
    public void CanMove_RejectsSameStatus()
    {
        TaskTransitions.CanMove(WorkTaskStatus.ToDo, WorkTaskStatus.ToDo).Should().BeFalse();
        TaskTransitions.CanMove(WorkTaskStatus.ToDo, WorkTaskStatus.InProgress).Should().BeTrue();
    }

    [Theory]
    [InlineData(WorkTaskStatus.InProgress, true)]
    [InlineData(WorkTaskStatus.Review, true)]
    [InlineData(WorkTaskStatus.ToDo, false)]
    [InlineData(WorkTaskStatus.Completed, false)]
    public void RequiresUnblocked_OnlyForActiveWork(WorkTaskStatus to, bool expected)
    {
        TaskTransitions.RequiresUnblocked(to).Should().Be(expected);
    }
}
