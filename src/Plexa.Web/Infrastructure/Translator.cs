using System.Globalization;
using Microsoft.Extensions.Localization;
using Plexa.Application.Abstractions;

namespace Plexa.Web.Infrastructure;

/// <summary>Implementación de <see cref="ITranslator"/> sobre los recursos compartidos de la web.</summary>
public sealed class Translator : ITranslator
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public Translator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string this[string text] => _localizer[text].Value;

    public string Format(string template, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, _localizer[template].Value, args);
}
