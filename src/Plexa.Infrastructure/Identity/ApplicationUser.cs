using Microsoft.AspNetCore.Identity;
using Plexa.Domain.Enums;

namespace Plexa.Infrastructure.Identity;

/// <summary>Usuario del espacio de trabajo (Identity + datos de perfil).</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
    public decimal CapacityHoursPerWeek { get; set; } = 40m;
    public string? AvatarPath { get; set; }
    public Guid WorkspaceId { get; set; }
    public string? Locale { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => string.Concat(parts[0][..1], parts[^1][..1]).ToUpperInvariant()
            };
        }
    }

    public bool IsAdmin => Role == WorkspaceRole.Admin;
}
