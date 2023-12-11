namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Model for DropDown type.
/// </summary>
public interface IComponentDropDown
{
  /// <summary>
  /// The title to display.
  /// </summary>
  public string Title { get; set; }
}

public interface IComponentDropDown<TContent> : IComponentDropDown
where TContent : IRichTextContent
{

  /// <summary>
  /// The Content to display.
  /// </summary>
  public TContent Content { get; set; }
}