using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Enums;
using Plexa.Web.Infrastructure;
using Plexa.Web.Models;

namespace Plexa.Web.Controllers;

[Authorize]
public sealed class CalendarController : Controller
{
    private readonly ICalendarService _calendar;
    private readonly IProjectService _projects;
    private readonly IUserContext _user;
    private readonly Plexa.Infrastructure.PlexaDbContext _db;

    public CalendarController(ICalendarService calendar, IProjectService projects, IUserContext user, Plexa.Infrastructure.PlexaDbContext db)
    {
        _calendar = calendar;
        _projects = projects;
        _user = user;
        _db = db;
    }

    [HttpGet("/calendar")]
    public async Task<IActionResult> Index(string view = "week", DateOnly? date = null, CancellationToken ct = default)
    {
        var model = await BuildAsync(view, date, ct);
        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "calendar-body")
        {
            return PartialView("_CalendarBody", model);
        }

        return View(model);
    }

    [HttpGet("/calendar/new")]
    public async Task<IActionResult> New(DateOnly? date, Guid? project, CancellationToken ct)
    {
        var model = await BuildAsync("week", date, ct);
        ViewData["Form"] = new CalendarEventFormModel
        {
            Date = date ?? DateOnly.FromDateTime(DateTime.Now),
            ProjectId = project
        };
        return PartialView("_EventFormModal", model);
    }

    [HttpPost("/calendar/events")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CalendarEventFormModel form, CancellationToken ct)
    {
        if (form.EndsAt <= form.StartsAt)
        {
            ModelState.AddModelError(nameof(form.EndsAt), "The end time must be after the start time.");
        }

        if (!ModelState.IsValid)
        {
            var model = await BuildAsync("week", form.Date, ct);
            ViewData["Form"] = form;
            Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            return PartialView("_EventFormModal", model);
        }

        var result = await _calendar.CreateAsync(form, _user.UserId, ct);
        if (!result.Ok)
        {
            var model = await BuildAsync("week", form.Date, ct);
            ViewData["Form"] = form;
            Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be created.");
            return PartialView("_EventFormModal", model);
        }

        Response.Headers["HX-Trigger"] = "{\"calendar:changed\":true,\"closeModal\":true}";
        Response.Toast("success", "Event created");
        return NoContent();
    }

    [HttpGet("/calendar/events/{id:guid}")]
    public async Task<IActionResult> Event(Guid id, CancellationToken ct)
    {
        var model = await _calendar.GetEventAsync(id, ct);
        return model is null ? NotFound() : PartialView("_EventDetailModal", model);
    }

    [HttpGet("/api/v1/calendar/events")]
    public async Task<IActionResult> ApiEvents(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var start = from ?? DateOnly.FromDateTime(DateTime.Now).AddDays(-7);
        var end = to ?? start.AddDays(30);
        return Ok(await _calendar.GetRangeAsync(start, end, ct));
    }

    private async Task<CalendarPageViewModel> BuildAsync(string view, DateOnly? date, CancellationToken ct)
    {
        var people = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_db.Users)
                .Where(u => u.WorkspaceId == _user.WorkspaceId)
                .OrderBy(u => u.FullName), ct);

        var departments = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_db.Departments)
                .Where(d => d.WorkspaceId == _user.WorkspaceId)
                .OrderBy(d => d.Name)
                .Select(d => new ValueTuple<Guid, string>(d.Id, d.Name)), ct);

        return new CalendarPageViewModel
        {
            Calendar = await _calendar.GetAsync(view, date, ct),
            People = people.Select(u => PersonChip.From(u.Id, u.FullName, u.JobTitle)).ToList(),
            Departments = departments.Select(d => (d.Item1, d.Item2)).ToList(),
            Projects = await _projects.GetVisibleProjectsAsync(ct)
        };
    }
}

[Authorize]
public sealed class MessagesController : Controller
{
    private readonly IMessageService _messages;
    private readonly IProjectService _projects;
    private readonly IUserContext _user;

    public MessagesController(IMessageService messages, IProjectService projects, IUserContext user)
    {
        _messages = messages;
        _projects = projects;
        _user = user;
    }

    [HttpGet("/messages")]
    public async Task<IActionResult> Index(Guid? id, CancellationToken ct)
    {
        var threads = await _messages.GetThreadsAsync(ct);
        var activeId = id ?? threads.FirstOrDefault()?.Id;
        var thread = activeId.HasValue ? await _messages.GetThreadAsync(activeId.Value, ct) : null;
        if (activeId.HasValue)
        {
            await _messages.MarkReadAsync(activeId.Value, ct);
        }

        var model = new MessagesPageViewModel
        {
            Threads = threads,
            Thread = thread,
            CanCreate = _user.IsAdmin
        };

        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "thread-panel")
        {
            return PartialView("_ThreadPanel", model);
        }

        return View(model);
    }

    [HttpGet("/messages/{id:guid}")]
    public async Task<IActionResult> Thread(Guid id, CancellationToken ct)
    {
        var threads = await _messages.GetThreadsAsync(ct);
        var thread = await _messages.GetThreadAsync(id, ct);
        if (thread is null)
        {
            return NotFound();
        }

        await _messages.MarkReadAsync(id, ct);
        return PartialView("_ThreadPanel", new MessagesPageViewModel { Threads = threads, Thread = thread, CanCreate = _user.IsAdmin });
    }

    [HttpPost("/messages/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(Guid id, string body, CancellationToken ct)
    {
        var result = await _messages.PostMessageAsync(id, body, _user.UserId, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? "The message could not be sent.");
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"notifications:changed\":true}";
        var thread = await _messages.GetThreadAsync(id, ct);
        return thread is null ? NoContent() : PartialView("_Messages", thread);
    }

    [HttpGet("/messages/new")]
    public async Task<IActionResult> New(CancellationToken ct)
    {
        if (!_user.IsAdmin)
        {
            return Forbid();
        }

        ViewData["Threads"] = await _messages.GetThreadsAsync(ct);
        ViewData["Projects"] = await _projects.GetVisibleProjectsAsync(ct);
        return PartialView("_ThreadFormModal");
    }

    [HttpPost("/messages/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, ThreadScope scope, Guid? projectId, CancellationToken ct)
    {
        var result = await _messages.CreateThreadAsync(title, scope, projectId, _user.UserId, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? "The channel could not be created.");
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"closeModal\":true,\"threads:changed\":true}";
        Response.Toast("success", "Channel created");
        return NoContent();
    }
}

[Authorize]
public sealed class NotificationsController : Controller
{
    private readonly INotificationService _notifications;
    private readonly IMessageService _messages;

    public NotificationsController(INotificationService notifications, IMessageService messages)
    {
        _notifications = notifications;
        _messages = messages;
    }

    [HttpGet("/notifications/panel")]
    public async Task<IActionResult> Panel(CancellationToken ct)
    {
        var items = await _notifications.GetRecentAsync(12, ct);
        ViewData["Unread"] = items.Count(i => !i.IsRead);
        await _notifications.MarkAllReadAsync(ct);
        return PartialView("_NotificationPanel", items);
    }

    [HttpGet("/api/v1/notifications")]
    public async Task<IActionResult> Api(CancellationToken ct)
    {
        var items = await _notifications.GetRecentAsync(12, ct);
        return Ok(new
        {
            unread = items.Count(i => !i.IsRead),
            messages = await _messages.UnreadCountAsync(ct),
            items
        });
    }

    [HttpGet("/notifications/unread")]
    public async Task<IActionResult> Unread(CancellationToken ct)
    {
        var count = await _notifications.UnreadCountAsync(ct);
        var messages = await _messages.UnreadCountAsync(ct);
        return Json(new { notifications = count, messages });
    }

    [HttpPost("/notifications/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReadAll(CancellationToken ct)
    {
        await _notifications.MarkAllReadAsync(ct);
        Response.Headers["HX-Trigger"] = "{\"notifications:changed\":true}";
        return NoContent();
    }
}

[Authorize]
public sealed class SearchController : Controller
{
    private readonly ISearchService _search;

    public SearchController(ISearchService search)
    {
        _search = search;
    }

    [HttpGet("/search/suggest")]
    public async Task<IActionResult> Suggest(string? q, CancellationToken ct) =>
        PartialView("_SearchSuggestions", await _search.SuggestAsync(q ?? string.Empty, ct));
}
