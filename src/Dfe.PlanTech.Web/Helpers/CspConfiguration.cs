namespace Dfe.PlanTech.Web.Helpers;

public sealed class CspConfiguration
{
    private const string CONFIG_KEY = "CSP";

    public readonly string ImgSrc;
    public readonly string ConnectSrc;
    public readonly string FrameSrc;
    public readonly string ScriptSrc;
    public readonly string DefaultSrc;

    public CspConfiguration(IConfiguration configuration)
    {
        ImgSrc = configuration.GetValue<string>($"{CONFIG_KEY}:ImgSrc") ?? "";
        ConnectSrc = configuration.GetValue<string>($"{CONFIG_KEY}:ConnectSrc") ?? "";
        FrameSrc = configuration.GetValue<string>($"{CONFIG_KEY}:FrameSrc") ?? "";
        ScriptSrc = configuration.GetValue<string>($"{CONFIG_KEY}:ScriptSrc") ?? "";
        DefaultSrc = configuration.GetValue<string>($"{CONFIG_KEY}:DefaultSrc") ?? "";
    }
}
