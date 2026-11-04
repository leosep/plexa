using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Plexa.Application.Abstractions;

namespace Plexa.Infrastructure.Services;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IConfiguration configuration)
    {
        var configured = configuration["Storage:Root"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "storage")
            : Path.GetFullPath(configured);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var key = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = File.Create(path);
        await content.CopyToAsync(stream, cancellationToken);
        return key.Replace('\\', '/');
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_root, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(path));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_root, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public string? GetPublicUrl(string storageKey) => $"/media/file/{Uri.EscapeDataString(storageKey)}";

    public string Root => _root;
}

public sealed class DataProtectionSecretProtector : ISecretProtector
{
    private readonly Microsoft.AspNetCore.DataProtection.IDataProtector _protector;

    public DataProtectionSecretProtector(Microsoft.AspNetCore.DataProtection.IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Plexa.Mailbox.v1");
    }

    public string Protect(string value) => _protector.Protect(value);

    public string? Unprotect(string value)
    {
        try
        {
            return _protector.Unprotect(value);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }
}

/// <summary>Envía correo por SMTP; en Development escribe el mensaje en el log.</summary>
public sealed class SmtpMailer : IMailer
{
    private readonly IConfiguration _configuration;
    private readonly PlexaDbContext _db;
    private readonly ISecretProtector _protector;
    private readonly ILogger<SmtpMailer> _logger;

    public SmtpMailer(IConfiguration configuration, PlexaDbContext db, ISecretProtector protector, ILogger<SmtpMailer> logger)
    {
        _configuration = configuration;
        _db = db;
        _protector = protector;
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default) =>
        SendCoreAsync(to, subject, htmlBody, null, cancellationToken);

    public async Task SendFromUserMailboxAsync(Guid userId, string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var mailbox = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(_db.UserMailboxes, m => m.UserId == userId, cancellationToken);
        await SendCoreAsync(to, subject, htmlBody, mailbox, cancellationToken);
    }

    private async Task SendCoreAsync(string to, string subject, string htmlBody, Domain.Entities.UserMailbox? mailbox, CancellationToken ct)
    {
        var host = mailbox?.SmtpHost ?? _configuration["Smtp:Host"];
        var port = mailbox?.SmtpPort ?? _configuration.GetValue("Smtp:Port", 587);
        var from = mailbox?.Email ?? _configuration["Smtp:From"] ?? "no-reply@plexa.app";
        var password = mailbox?.ProtectedPassword is not null ? _protector.Unprotect(mailbox.ProtectedPassword) : _configuration["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("[MAIL:console] To={To} From={From} Subject={Subject}\n{Body}", to, from, subject, htmlBody);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, mailbox?.UseStartTls == false ? SecureSocketOptions.None : SecureSocketOptions.StartTlsWhenAvailable, ct);
        await client.AuthenticateAsync(from, password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
