using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Content.Models;

public class RichTextContentIdWithContentComponentId
{
  [Key]
  public long Id { get; set; }

  public string ContentId { get; set; } = null!;

  public long DataId { get; set; }

  public long? ParentId { get; set; }
}