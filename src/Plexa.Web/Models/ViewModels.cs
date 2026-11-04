using System.ComponentModel.DataAnnotations;
using Plexa.Domain.Enums;

namespace Plexa.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool KeepSignedIn { get; set; } = true;

    public string? ReturnUrl { get; set; }
}

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repeat the password.")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class InvitationViewModel
{
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the invitation code.")]
    public string Code { get; set; } = string.Empty;

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    [Required(ErrorMessage = "Tell us your name.")]
    public string FullName { get; set; } = string.Empty;

    public string? JobTitle { get; set; }

    [Required(ErrorMessage = "Choose a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool Invalid { get; set; }
}

public sealed class TaskFormViewModel
{
    public Plexa.Application.Dtos.TaskFormModel Form { get; set; } = new();
    public Plexa.Application.Dtos.TaskFormOptionsDto Options { get; set; } = new([], [], [], new());
    public string? Error { get; set; }
    public bool IsEdit => Form.Id.HasValue;
    public bool IsSubtask => Form.ParentId.HasValue;
    public Guid? ParentId { get; set; }
}

public sealed class SettingsViewModel
{
    public string Section { get; set; } = "profile";
    public Plexa.Application.Dtos.ProfileDto Profile { get; set; } = null!;
    public Plexa.Application.Dtos.MailboxDto Mailbox { get; set; } = null!;
    public string? Error { get; set; }
    public string? Success { get; set; }
}

public sealed class ReportPageViewModel
{
    public Plexa.Application.Dtos.ReportPageDto Page { get; set; } = null!;
}

public sealed class BoardPageViewModel
{
    public Plexa.Application.Dtos.BoardDto Board { get; set; } = null!;
    public Plexa.Application.Dtos.TaskDetailDto? Detail { get; set; }
    public bool CanApprove { get; set; }
}

public sealed class CalendarPageViewModel
{
    public Plexa.Application.Dtos.CalendarDto Calendar { get; set; } = null!;
    public IReadOnlyList<Plexa.Application.Dtos.PersonChip> People { get; set; } = [];
    public IReadOnlyList<(Guid Id, string Name)> Departments { get; set; } = [];
    public IReadOnlyList<Plexa.Application.Dtos.ProjectOption> Projects { get; set; } = [];
}

public sealed class MessagesPageViewModel
{
    public IReadOnlyList<Plexa.Application.Dtos.ThreadSummaryDto> Threads { get; set; } = [];
    public Plexa.Application.Dtos.ThreadDto? Thread { get; set; }
    public bool CanCreate { get; set; }
}

public sealed class AdminSprintsViewModel
{
    public IReadOnlyList<Plexa.Application.Services.SprintDto> Sprints { get; set; } = [];
}

public sealed class AdminMembersViewModel
{
    public IReadOnlyList<Plexa.Application.Services.AdminMemberDto> Members { get; set; } = [];
}

public sealed class AdminProjectsViewModel
{
    public IReadOnlyList<Plexa.Application.Services.AdminProjectDto> Projects { get; set; } = [];
}
