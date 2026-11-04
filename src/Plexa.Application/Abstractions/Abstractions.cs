using Plexa.Domain.Enums;

namespace Plexa.Application.Abstractions;

/// <summary>Reloj inyectable (nunca DateTime.Now).</summary>
public interface IClock
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}

/// <summary>Usuario y espacio activos de la petición.</summary>
public interface IUserContext
{
    Guid UserId { get; }
    Guid WorkspaceId { get; }
    string FullName { get; }
    string? Email { get; }
    string? JobTitle { get; }
    string? AvatarPath { get; }
    WorkspaceRole Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

public interface IMailer
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendFromUserMailboxAsync(Guid userId, string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    string? GetPublicUrl(string storageKey);
}

/// <summary>Cifra secretos (contraseña del buzón) sin exponerlos nunca.</summary>
public interface ISecretProtector
{
    string Protect(string value);
    string? Unprotect(string value);
}

public interface IAppLinkGenerator
{
    string AbsoluteUrl(string relativePath);
}
