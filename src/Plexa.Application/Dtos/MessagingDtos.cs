using Plexa.Domain.Enums;

namespace Plexa.Application.Dtos;

public sealed record ThreadSummaryDto(
    Guid Id,
    string Title,
    ThreadScope Scope,
    string ScopeLabel,
    string? ProjectLabel,
    string? LastMessagePreview,
    string? LastAuthor,
    DateTime? LastAt,
    bool Unread);

public sealed record MessageDto(
    Guid Id,
    string Body,
    PersonChip Author,
    DateTime CreatedAt,
    string TimeLabel,
    bool IsMine,
    IReadOnlyList<string> Mentions);

public sealed record ThreadDto(
    Guid Id,
    string Title,
    string ScopeLabel,
    string? ProjectLabel,
    IReadOnlyList<MessageDto> Messages,
    bool CanPost);

public sealed record NotificationDto(
    Guid Id,
    string Text,
    string? ActorName,
    DateTime CreatedAt,
    string RelativeLabel,
    bool IsRead,
    string? Url,
    NotificationType Type);

public sealed record ProfileDto(PersonChip Person, string Email, string RoleLabel, string? JobTitle, string? AvatarPath);

public sealed record MailboxDto(string Email, string SmtpHost, int SmtpPort, bool UseStartTls, bool HasPassword);
