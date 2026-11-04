using Microsoft.AspNetCore.Mvc;
using Plexa.Application.Abstractions;
using Plexa.Application.Services;

namespace Plexa.Web.ViewComponents;

public sealed class AvatarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string name, string? path = null, int size = 32)
    {
        var initials = string.Concat(name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));

        return View(new AvatarModel(name, initials, path, size));
    }

    public sealed record AvatarModel(string Name, string Initials, string? Path, int Size);
}

public sealed class NotificationBellViewComponent : ViewComponent
{
    private readonly INotificationService _notifications;
    private readonly IMessageService _messages;

    public NotificationBellViewComponent(INotificationService notifications, IMessageService messages)
    {
        _notifications = notifications;
        _messages = messages;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken ct)
    {
        var unread = await _notifications.UnreadCountAsync(ct);
        var messages = await _messages.UnreadCountAsync(ct);
        return View(new BellModel(unread, messages));
    }

    public sealed record BellModel(int Unread, int Messages);
}

public sealed class UserMenuViewComponent : ViewComponent
{
    private readonly IUserContext _user;

    public UserMenuViewComponent(IUserContext user)
    {
        _user = user;
    }

    public IViewComponentResult Invoke() =>
        View(new UserMenuModel(_user.FullName, _user.Email ?? string.Empty, _user.JobTitle, _user.AvatarPath, _user.IsAdmin));
}

public sealed record UserMenuModel(string FullName, string Email, string? JobTitle, string? AvatarPath, bool IsAdmin);
