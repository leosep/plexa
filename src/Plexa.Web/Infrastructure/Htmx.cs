using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Plexa.Web.Infrastructure;

public static class HtmxExtensions
{
    /// <summary>True cuando la petición viene de htmx (HX-Request).</summary>
    public static bool IsHtmx(this HttpRequest request) =>
        request.Headers.ContainsKey("HX-Request") &&
        !string.Equals(request.Headers["HX-Request"], "false", StringComparison.OrdinalIgnoreCase);

    public static bool IsBoosted(this HttpRequest request) =>
        request.IsHtmx() && request.Headers.ContainsKey("HX-Boosted");

    public static void Toast(this HttpResponse response, string type, string message, string? undoUrl = null)
    {
        var text = response.HttpContext.Message(message);
        var payload = new Dictionary<string, JsonElement>
        {
            ["toast"] = JsonSerializer.SerializeToElement(new { type, message = text, undoUrl })
        };

        // Se conserva cualquier evento ya encolado (task:changed, closeModal, …): htmx solo lee una cabecera.
        var existing = response.Headers["HX-Trigger"].ToString();
        if (!string.IsNullOrWhiteSpace(existing))
        {
            try
            {
                var previous = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existing);
                if (previous is not null)
                {
                    foreach (var pair in previous)
                    {
                        payload[pair.Key] = pair.Value;
                    }
                }
            }
            catch (JsonException)
            {
                // Cabecera no JSON: se descarta para no romper el toast.
            }
        }

        response.Headers["HX-Trigger"] = JsonSerializer.Serialize(payload);
    }

    public static void Trigger(this HttpResponse response, string eventName, object? payload = null)
    {
        var json = payload is null
            ? JsonSerializer.Serialize(new Dictionary<string, object> { [eventName] = true })
            : JsonSerializer.Serialize(new Dictionary<string, object> { [eventName] = payload });
        response.Headers["HX-Trigger"] = json;
    }
}

/// <summary>Valida el anti-forgery en todos los POST y devuelve 422 con el parcial en errores de validación.</summary>
public sealed class HtmxValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Result is ViewResult view && !view.ViewData.ModelState.IsValid && context.HttpContext.Request.IsHtmx())
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
    }
}

public static class ViewHelpers
{
    public static string ActiveClass(this HttpRequest request, string path)
    {
        var current = request.Path.Value ?? string.Empty;
        if (path == "/")
        {
            return current == "/" ? "is-active" : string.Empty;
        }

        return current.StartsWith(path, StringComparison.OrdinalIgnoreCase) ? "is-active" : string.Empty;
    }

    public static string FormatDuration(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = value.Duration();
            return "-" + Plexa.Domain.Rules.DurationFormatter.Format(value);
        }

        return Plexa.Domain.Rules.DurationFormatter.Format(value);
    }
}
