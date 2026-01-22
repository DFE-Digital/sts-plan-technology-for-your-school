using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record ContentSecurityPolicyConfiguration(IConfiguration configuration)
{
    private const string CONFIG_KEY = "CSP";

    public readonly string ImgSrc =
        configuration.GetValue<string>($"{CONFIG_KEY}:ImgSrc") ?? string.Empty;
    public readonly string ConnectSrc =
        configuration.GetValue<string>($"{CONFIG_KEY}:ConnectSrc") ?? string.Empty;
    public readonly string FrameSrc =
        configuration.GetValue<string>($"{CONFIG_KEY}:FrameSrc") ?? string.Empty;
    public readonly string ScriptSrc =
        configuration.GetValue<string>($"{CONFIG_KEY}:ScriptSrc") ?? string.Empty;
    public readonly string DefaultSrc =
        configuration.GetValue<string>($"{CONFIG_KEY}:DefaultSrc") ?? string.Empty;
}
