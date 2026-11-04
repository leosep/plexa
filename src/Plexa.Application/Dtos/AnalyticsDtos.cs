using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record AnalyticsKpiDto(string Label, string Value, string Delta, bool Positive, string? Hint = null);

public sealed record WeekSeriesDto(string Label, IReadOnlyList<int> Values);

public sealed record CumulativeFlowDto(
    IReadOnlyList<string> Labels,
    IReadOnlyList<WeekSeriesDto> Series,
    IReadOnlyList<StatusSlice> Shares,
    int TrackedTasks);

public sealed record ThroughputDto(IReadOnlyList<string> Labels, IReadOnlyList<int> Counts);

public sealed record EstimateAccuracyDto(IReadOnlyList<string> Labels, IReadOnlyList<decimal> Estimated, IReadOnlyList<decimal> Logged, bool IsEmpty);

public sealed record LoadBubbleDto(Guid UserId, string Name, string Initials, int ActiveTasks);

public sealed record ReworkOriginDto(int ReopenedCount, IReadOnlyList<(string Label, int Count)> Origins);

public sealed record SprintVelocityDto(IReadOnlyList<string> Labels, IReadOnlyList<int> Committed, IReadOnlyList<int> Delivered, bool IsEmpty);

public sealed record AnalyticsDto(
    IReadOnlyList<AnalyticsKpiDto> Kpis,
    CumulativeFlowDto CumulativeFlow,
    ThroughputDto Throughput,
    EstimateAccuracyDto EstimateAccuracy,
    IReadOnlyList<LoadBubbleDto> LoadPerMember,
    ReworkOriginDto ReworkOrigin,
    SprintVelocityDto SprintVelocity);
