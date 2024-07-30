namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Model for Warning Component content type
/// </summary>
public interface IWarningComponent
{

}


public interface IWarningComponent<out TTextBody> : IWarningComponent
where TTextBody : ITextBody
{
    /// <summary>
    /// Warning text
    /// </summary>
    public TTextBody Text { get; }
}
