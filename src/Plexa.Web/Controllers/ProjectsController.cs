using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plexa.Application.Abstractions;
using Plexa.Application.Dtos;
using Plexa.Application.Services;
using Plexa.Domain.Enums;
using Plexa.Web.Infrastructure;

namespace Plexa.Web.Controllers;

[Authorize]
public sealed class ProjectsController : Controller
{
    private readonly IProjectService _projects;
    private readonly IUserContext _user;
    private readonly ITranslator _t;

    public ProjectsController(IProjectService projects, IUserContext user, ITranslator translator)
    {
        _projects = projects;
        _user = user;
        _t = translator;
    }

    [HttpGet("/projects")]
    public async Task<IActionResult> Index(ProjectStatus? status, string? q, CancellationToken ct)
    {
        var model = await _projects.ListAsync(status, q, ct);
        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "project-list")
        {
            return PartialView("_ProjectList", model);
        }

        return View(model);
    }

    [HttpGet("/projects/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, string tab = "Overview", CancellationToken ct = default)
    {
        var model = await _projects.GetDetailAsync(id, tab, ct);
        if (model is null)
        {
            return NotFound();
        }

        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "project-tab")
        {
            return PartialView("_ProjectTab", model);
        }

        return View(model);
    }

    [HttpGet("/projects/{id:guid}/team/edit")]
    public async Task<IActionResult> EditTeam(Guid id, CancellationToken ct)
    {
        var model = await _projects.GetTeamEditAsync(id, ct);
        return model is null ? Forbid() : PartialView("_TeamEditModal", model);
    }

    [HttpPost("/projects/{id:guid}/team")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTeam(Guid id, TeamEditModel model, CancellationToken ct)
    {
        model.ProjectId = id;
        var result = await _projects.SaveTeamAsync(model, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The team could not be saved."]);
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"closeModal\":true,\"project:changed\":true}";
        Response.Toast("success", _t["Team updated"]);
        return NoContent();
    }

    [HttpPost("/projects/{id:guid}/documents")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(26L * 1024 * 1024)]
    public async Task<IActionResult> UploadDocuments(Guid id, List<IFormFile> files, CancellationToken ct)
    {
        var payload = new List<(string, string, long, Stream)>();
        foreach (var file in files.Where(f => f.Length > 0))
        {
            payload.Add((file.FileName, file.ContentType, file.Length, file.OpenReadStream()));
        }

        var result = await _projects.UploadDocumentsAsync(id, payload, _user.UserId, ct);
        foreach (var item in payload)
        {
            await item.Item4.DisposeAsync();
        }

        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The files could not be uploaded."]);
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"project:changed\":true}";
        Response.Toast("success", _t["Documents uploaded"]);
        var detail = await _projects.GetDetailAsync(id, "Documents", ct);
        return detail is null ? NoContent() : PartialView("_ProjectTab", detail);
    }

    [HttpPost("/projects/{id:guid}/documents/{documentId:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var result = await _projects.DeleteDocumentAsync(documentId, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The document could not be deleted."]);
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"project:changed\":true}";
        Response.Toast("success", _t["Document deleted"]);
        var detail = await _projects.GetDetailAsync(id, "Documents", ct);
        return detail is null ? NoContent() : PartialView("_ProjectTab", detail);
    }
}

[Authorize]
public sealed class MediaController : Controller
{
    private readonly IProjectService _projects;

    public MediaController(IProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet("/media/{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await _projects.OpenDocumentAsync(id, ct);
        return file is null ? NotFound() : File(file.Value.Content, file.Value.ContentType, file.Value.FileName, enableRangeProcessing: true);
    }
}

[Authorize]
public sealed class ReportsController : Controller
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("/reports")]
    public async Task<IActionResult> Index(string? sprint, Guid? member, string? tag, bool timeLogged = false, bool variance = false, CancellationToken ct = default)
    {
        var filters = new ReportFilters(sprint, member, tag, timeLogged, variance);
        var page = await _reports.GetAsync(filters, ct);
        if (Request.IsHtmx() && Request.Headers["HX-Target"] == "report-body")
        {
            return PartialView("_ReportBody", page);
        }

        return View(page);
    }

    [HttpGet("/reports/export")]
    public async Task<IActionResult> Export(string format, string? sprint, Guid? member, string? tag, bool timeLogged = false, bool variance = false, CancellationToken ct = default)
    {
        var filters = new ReportFilters(sprint, member, tag, timeLogged, variance);
        var bytes = await _reports.ExportAsync(filters, format, ct);
        var (contentType, extension) = format.ToLowerInvariant() switch
        {
            "csv" => ("text/csv", "csv"),
            "xlsx" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            "pdf" => ("application/pdf", "pdf"),
            _ => ("application/octet-stream", "bin")
        };

        return File(bytes, contentType, $"plexa-hours-report-{DateTime.UtcNow:yyyyMMdd}.{extension}");
    }
}
