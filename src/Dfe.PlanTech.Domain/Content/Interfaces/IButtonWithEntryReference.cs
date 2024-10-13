namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// A button that links to a different entry
/// </summary>
public interface IButtonWithEntryReference
{
}

public interface IButtonWithEntryReference<out TButton, out TContent> : IButtonWithEntryReference
where TButton : IButton
where TContent : IContentComponentType
{
    public TButton? Button { get; }

    /// <summary>
    /// What content this button should link to
    /// </summary>
    public TContent? LinkToEntry { get; }
}
