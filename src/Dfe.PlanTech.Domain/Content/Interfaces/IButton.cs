namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Base information for a button
/// </summary>
public interface IButton
{
    /// <summary>
    /// Text value of the button
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Whether this button is a start button or not; see GDS button for information
    /// </summary>
    public bool IsStartButton { get; }
}
