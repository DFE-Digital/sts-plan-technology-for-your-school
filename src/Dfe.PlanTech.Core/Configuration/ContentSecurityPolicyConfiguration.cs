using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration;

[ExcludeFromCodeCoverage]
public record ContentSecurityPolicyConfiguration()
{
    public string ImgSrc { get; set; } = string.Empty;
    public string ConnectSrc { get; set; } = string.Empty;
    public string FrameSrc { get; set; } = string.Empty;
    public string ScriptSrc { get; set; } = string.Empty;
    public string DefaultSrc { get; set; } = string.Empty;
}
