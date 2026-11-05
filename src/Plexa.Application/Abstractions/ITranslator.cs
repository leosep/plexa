namespace Plexa.Application.Abstractions;

/// <summary>
/// Traducción de textos generados en la capa de servicios (mensajes, etiquetas y
/// exportaciones). Usa el texto en inglés como clave: si no hay traducción para la
/// cultura actual se devuelve la propia clave, de modo que el inglés siempre funciona.
/// </summary>
public interface ITranslator
{
    /// <summary>Traduce una etiqueta o mensaje corto.</summary>
    string this[string text] { get; }

    /// <summary>Traduce una plantilla con marcadores <c>{0}</c>, <c>{1}</c>… y la formatea.</summary>
    string Format(string template, params object?[] args);
}
