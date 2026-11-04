using Microsoft.AspNetCore.Mvc;

namespace Plexa.Web;

/// <summary>Recurso compartido de textos de la interfaz (en/es).</summary>
public sealed class SharedResource
{
}

public sealed class ChipTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeName("tone")]
    public string? Tone { get; set; }

    public override void Process(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        var tone = Tone switch
        {
            "high" or "danger" => "bg-danger-100 text-danger-500",
            "medium" or "accent" => "bg-accent-100 text-accent-500",
            "low" => "bg-ink-100 text-ink-600",
            "brand" or "success" => "bg-brand-50 text-brand-700",
            "info" => "bg-cyan-100 text-cyan-500",
            _ => "bg-ink-100 text-ink-600"
        };

        output.TagName = "span";
        output.Attributes.Add("class", $"plx-chip {tone}");
    }
}
