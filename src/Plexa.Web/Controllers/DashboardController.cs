using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plexa.Application.Abstractions;
using Plexa.Application.Services;
using Plexa.Web.Infrastructure;

namespace Plexa.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly IDashboardService _dashboard;
    private readonly IUserContext _user;

    public DashboardController(IDashboardService dashboard, IUserContext user)
    {
        _dashboard = dashboard;
        _user = user;
    }

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _dashboard.GetAsync(_user.UserId, ct);
        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "dashboard-body")
        {
            return PartialView("_DashboardBody", model);
        }

        return View(model);
    }

    [HttpGet("/dashboard/card/{name}")]
    public async Task<IActionResult> Card(string name, CancellationToken ct)
    {
        var model = await _dashboard.GetAsync(_user.UserId, ct);
        return name switch
        {
            "status" => PartialView("_StatusCard", model),
            "activity" => PartialView("_ActivityCard", model),
            "priority" => PartialView("_PriorityCard", model),
            "today" => PartialView("_TodayCard", model),
            "workload" => PartialView("_WorkloadCard", model),
            _ => NotFound()
        };
    }
}

[Authorize]
public sealed class TeamController : Controller
{
    private readonly ICapacityService _capacity;

    public TeamController(ICapacityService capacity)
    {
        _capacity = capacity;
    }

    [HttpGet("/team")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await _capacity.GetTeamAsync(ct));
}

[Authorize]
public sealed class AnalyticsController : Controller
{
    private readonly IAnalyticsService _analytics;

    public AnalyticsController(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("/analytics")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await _analytics.GetAsync(ct));
}
