using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plexa.Application.Abstractions;
using Plexa.Application.Services;
using Plexa.Web.Infrastructure;

namespace Plexa.Web.Controllers;

[Authorize]
[Route("api/v1/time")]
public sealed class TimeApiController : Controller
{
    private readonly ITimerService _timer;
    private readonly IUserContext _user;

    public TimeApiController(ITimerService timer, IUserContext user)
    {
        _timer = timer;
        _user = user;
    }

    [HttpGet("timer")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var timer = await _timer.GetActiveAsync(_user.UserId, ct);
        return timer is null ? NoContent() : Ok(timer);
    }

    [HttpPost("timer/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid taskId, CancellationToken ct)
    {
        var timer = await _timer.StartAsync(taskId, _user.UserId, ct);
        Response.Headers["HX-Trigger"] = "{\"timer:changed\":true,\"task:changed\":true}";
        Response.Toast("success", "Timer started");
        return Ok(timer);
    }

    [HttpPost("timer/stop")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Stop(CancellationToken ct)
    {
        var timer = await _timer.StopAsync(_user.UserId, ct);
        Response.Headers["HX-Trigger"] = "{\"timer:changed\":true,\"task:changed\":true}";
        Response.Toast("success", "Timer stopped");
        return timer is null ? NoContent() : Ok(timer);
    }

    [HttpGet("today")]
    public async Task<IActionResult> Today(CancellationToken ct) =>
        Ok(await _timer.GetTodayAsync(_user.UserId, ct));
}

/// <summary>Acciones de cronómetro para navegadores sin JavaScript (progressive enhancement).</summary>
[Authorize]
public sealed class TimerController : Controller
{
    private readonly ITimerService _timer;
    private readonly IUserContext _user;

    public TimerController(ITimerService timer, IUserContext user)
    {
        _timer = timer;
        _user = user;
    }

    [HttpPost("/board/timer/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid id, string? returnUrl, CancellationToken ct)
    {
        await _timer.StartAsync(id, _user.UserId, ct);
        Response.Toast("success", "Timer started");
        return Redirect(SafeReturn(returnUrl));
    }

    [HttpPost("/board/timer/stop")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Stop(string? returnUrl, CancellationToken ct)
    {
        await _timer.StopAsync(_user.UserId, ct);
        Response.Toast("success", "Timer stopped");
        return Redirect(SafeReturn(returnUrl));
    }

    private string SafeReturn(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/board";
}
