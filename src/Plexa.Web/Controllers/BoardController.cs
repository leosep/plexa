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
public sealed class BoardController : Controller
{
    private readonly ITaskService _tasks;
    private readonly IProjectService _projects;
    private readonly IUserContext _user;
    private readonly ITranslator _t;

    public BoardController(ITaskService tasks, IProjectService projects, IUserContext user, ITranslator translator)
    {
        _tasks = tasks;
        _projects = projects;
        _user = user;
        _t = translator;
    }

    [HttpGet("/board")]
    public async Task<IActionResult> Index(Guid? project, bool loose = false, Guid? assignee = null, string? q = null, Guid? task = null, CancellationToken ct = default)
    {
        var board = await _tasks.GetBoardAsync(new BoardQuery(project, loose, assignee, q, task), ct);
        TaskDetailDto? detail = null;
        if (task.HasValue)
        {
            detail = await _tasks.GetDetailAsync(task.Value, ct);
        }

        var vm = new BoardPageViewModel { Board = board, Detail = detail, CanApprove = _user.IsAdmin };
        if (Request.IsHtmx() && !Request.IsBoosted() && Request.Headers.ContainsKey("HX-Target"))
        {
            var target = Request.Headers["HX-Target"].ToString();
            if (target is "board-results" or "board-content")
            {
                return PartialView("_Board", vm);
            }
        }

        return View(vm);
    }

    [HttpGet("/board/card/{id:guid}")]
    public async Task<IActionResult> Card(Guid id, CancellationToken ct)
    {
        var card = await _tasks.GetCardAsync(id, ct);
        return card is null ? NotFound() : PartialView("_TaskCard", card);
    }

    [HttpGet("/board/detail/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        var detail = await _tasks.GetDetailAsync(id, ct);
        if (detail is null)
        {
            return NotFound();
        }

        ViewData["CanApprove"] = _user.IsAdmin;
        return PartialView("_TaskDetailModal", detail);
    }

    [HttpGet("/tasks/{id:guid}")]
    public async Task<IActionResult> TaskPage(Guid id, CancellationToken ct)
    {
        var detail = await _tasks.GetDetailAsync(id, ct);
        if (detail is null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            ViewData["CanApprove"] = _user.IsAdmin;
            return PartialView("_TaskDetailModal", detail);
        }

        return Redirect($"/board?task={id}");
    }

    [HttpGet("/tasks/new")]
    public async Task<IActionResult> New(Guid? project = null, Guid? parent = null, CancellationToken ct = default)
    {
        var options = await _tasks.GetFormOptionsAsync(null, project, ct);
        var form = new TaskFormModel
        {
            ProjectId = project,
            ParentId = parent,
            EstimateHours = 4m,
            StoryPoints = 3,
            InBacklog = true
        };

        if (parent.HasValue)
        {
            var parentTask = await _tasks.GetDetailAsync(parent.Value, ct);
            if (parentTask is not null)
            {
                form.ProjectId = parentTask.ProjectId;
                form.Title = string.Empty;
            }
        }

        return PartialView("_TaskFormModal", new TaskFormViewModel { Form = form, Options = options, ParentId = parent });
    }

    [HttpPost("/tasks")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskFormModel form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await FormErrorAsync(form, null, ct);
        }

        var result = await _tasks.CreateAsync(form, _user.UserId, ct);
        if (!result.Ok)
        {
            return await FormErrorAsync(form, result.Error, ct);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true,\"closeModal\":true}";
        Response.Toast("success", _t["Task created"]);
        return PartialView("_TaskCreated", result.TaskId);
    }

    [HttpGet("/tasks/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var form = await _tasks.GetFormAsync(id, ct);
        if (form is null)
        {
            return NotFound();
        }

        var options = await _tasks.GetFormOptionsAsync(id, form.ProjectId, ct);
        return PartialView("_TaskFormModal", new TaskFormViewModel { Form = form, Options = options, ParentId = form.ParentId });
    }

    [HttpPost("/tasks/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, TaskFormModel form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid)
        {
            return await FormErrorAsync(form, null, ct);
        }

        var result = await _tasks.UpdateAsync(form, _user.UserId, ct);
        if (!result.Ok)
        {
            return await FormErrorAsync(form, result.Error, ct);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true,\"closeModal\":true}";
        Response.Toast("success", _t["Task updated"]);
        return PartialView("_TaskCreated", id);
    }

    [HttpPost("/tasks/{id:guid}/delete")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _tasks.DeleteAsync(id, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The task could not be deleted."]);
            return StatusCode(StatusCodes.Status409Conflict);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true,\"closeModal\":true}";
        Response.Toast("success", _t["Task deleted"]);
        return NoContent();
    }

    [HttpPost("/board/move")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(Guid id, WorkTaskStatus status, string? source, CancellationToken ct)
    {
        var result = await _tasks.MoveAsync(id, status, _user.UserId, ct);
        if (!result.Success)
        {
            Response.Toast("error", result.Error ?? _t["The task cannot move yet."]);
            Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
            return StatusCode(StatusCodes.Status409Conflict);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        Response.Toast("success", result.Toast ?? _t["Task moved"]);
        if (source == "detail")
        {
            var detail = await _tasks.GetDetailAsync(id, ct);
            ViewData["CanApprove"] = _user.IsAdmin;
            return detail is null ? NoContent() : PartialView("_TaskDetailModal", detail);
        }

        return result.Card is null ? NoContent() : PartialView("_TaskCard", result.Card);
    }

    [HttpPost("/board/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _tasks.ApproveAsync(id, _user.UserId, ct);
        if (!result.Success)
        {
            Response.Toast("error", result.Error ?? _t["The task cannot be approved."]);
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        Response.Toast("success", _t["Task approved"]);
        var detail = await _tasks.GetDetailAsync(id, ct);
        ViewData["CanApprove"] = _user.IsAdmin;
        return detail is null ? NoContent() : PartialView("_TaskDetailModal", detail);
    }

    [HttpPost("/board/assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid id, Guid? assignee, CancellationToken ct)
    {
        var form = await _tasks.GetFormAsync(id, ct);
        if (form is null)
        {
            return NotFound();
        }

        form.AssigneeId = assignee;
        var result = await _tasks.UpdateAsync(form, _user.UserId, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The task could not be assigned."]);
            return StatusCode(StatusCodes.Status409Conflict);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        Response.Toast("success", assignee.HasValue ? _t["Task assigned"] : _t["Task left unassigned"]);
        var card = await _tasks.GetCardAsync(id, ct);
        return card is null ? NoContent() : PartialView("_TaskCard", card);
    }

    [HttpPost("/tasks/{id:guid}/subtasks")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubtask(Guid id, string title, decimal hours, Guid? assignee, CancellationToken ct)
    {
        var result = await _tasks.AddSubtaskAsync(id, title, hours, assignee, _user.UserId, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The subtask could not be created."]);
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        Response.Toast("success", _t["Subtask added"]);
        var detail = await _tasks.GetDetailAsync(id, ct);
        return detail is null ? NoContent() : PartialView("_TaskDetailBody", detail);
    }

    [HttpPost("/tasks/{id:guid}/checklist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddChecklist(Guid id, string text, CancellationToken ct)
    {
        var result = await _tasks.AddChecklistStepAsync(id, text, ct);
        if (!result.Ok)
        {
            Response.Toast("error", result.Error ?? _t["The step could not be added."]);
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        var detail = await _tasks.GetDetailAsync(id, ct);
        return detail is null ? NoContent() : PartialView("_TaskDetailBody", detail);
    }

    [HttpPost("/tasks/checklist/{stepId:guid}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStep(Guid stepId, Guid taskId, CancellationToken ct)
    {
        await _tasks.ToggleChecklistStepAsync(stepId, ct);
        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        var detail = await _tasks.GetDetailAsync(taskId, ct);
        return detail is null ? NoContent() : PartialView("_TaskDetailBody", detail);
    }

    [HttpPost("/tasks/checklist/{stepId:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStep(Guid stepId, Guid taskId, CancellationToken ct)
    {
        await _tasks.DeleteChecklistStepAsync(stepId, ct);
        Response.Headers["HX-Trigger"] = "{\"task:changed\":true}";
        var detail = await _tasks.GetDetailAsync(taskId, ct);
        return detail is null ? NoContent() : PartialView("_TaskDetailBody", detail);
    }

    [HttpGet("/board/projects")]
    public async Task<IActionResult> Projects(CancellationToken ct) =>
        Json(await _projects.GetVisibleProjectsAsync(ct));

    private async Task<IActionResult> FormErrorAsync(TaskFormModel form, string? error, CancellationToken ct)
    {
        var options = await _tasks.GetFormOptionsAsync(form.Id, form.ProjectId, ct);
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return PartialView("_TaskFormModal", new TaskFormViewModel
        {
            Form = form,
            Options = options,
            Error = error,
            ParentId = form.ParentId
        });
    }
}
