using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Plexa.Web.Infrastructure;

/// <summary>
/// Traducción de textos dinámicos (etiquetas de estado, mensajes de servicio y toasts).
/// Se usa el propio texto en inglés como clave: si no existe traducción, el localizador
/// devuelve la clave, de modo que la interfaz sigue siendo legible en inglés.
/// Los mensajes con datos usan el formato <c>"texto {0}|valor"</c>.
/// </summary>
public static class LocalizationExtensions
{
    public static string Label(this IStringLocalizer localizer, string? text) =>
        string.IsNullOrWhiteSpace(text) ? string.Empty : localizer[text].Value;

    public static string Message(this IStringLocalizer localizer, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var separator = raw.IndexOf('|');
        if (separator < 0)
        {
            return localizer[raw].Value;
        }

        var templateText = raw[..separator];
        var translated = localizer[templateText];
        var format = translated.ResourceNotFound ? templateText : translated.Value;
        var args = raw[(separator + 1)..].Split('|').Cast<object>().ToArray();
        return string.Format(CultureInfo.CurrentCulture, format, args);
    }

    public static string Message(this HttpContext context, string? raw)
    {
        var localizer = context.RequestServices.GetService<IStringLocalizer<SharedResource>>();
        return localizer is null ? raw ?? string.Empty : localizer.Message(raw);
    }
}
