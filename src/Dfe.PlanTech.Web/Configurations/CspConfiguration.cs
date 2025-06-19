namespace Dfe.PlanTech.Web.Configurations;

public record CspConfiguration(IConfiguration configuration)
{
    private const string CONFIG_KEY = "CSP";

    public readonly string ImgSrc = configuration.GetValue<string>($"{CONFIG_KEY}:ImgSrc") ?? "";
    public readonly string ConnectSrc = configuration.GetValue<string>($"{CONFIG_KEY}:ConnectSrc") ?? "";
    public readonly string FrameSrc = configuration.GetValue<string>($"{CONFIG_KEY}:FrameSrc") ?? "";
    public readonly string ScriptSrc = configuration.GetValue<string>($"{CONFIG_KEY}:ScriptSrc") ?? "";
    public readonly string DefaultSrc = configuration.GetValue<string>($"{CONFIG_KEY}:DefaultSrc") ?? "";
}
