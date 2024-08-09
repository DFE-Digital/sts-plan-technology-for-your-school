namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// A button that links to a URL
/// </summary>
public interface IButtonWithLink
{
    /// <summary>
    /// HREF for the button - the URL where it should link to
    /// </summary>
    public string? Href { get; }
}

public interface IButtonWithLink<out TButton> : IButtonWithLink
where TButton : IButton
{
    public TButton Button { get; }
}

