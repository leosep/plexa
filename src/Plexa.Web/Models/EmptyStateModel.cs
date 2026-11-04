namespace Plexa.Web.Models;

public sealed record EmptyStateModel(string Title, string? Body = null, string? CtaLabel = null, string? CtaHref = null, string? Icon = null);
