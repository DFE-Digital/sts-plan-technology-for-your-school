using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Category : ICategory
{
    public string Name { get; set; } = null!;

    public string Title { get; set; } = null!;

    //This should be "Document" type from Contentful, as is Rich Text
    public object? Description { get; set; }

    public List<IQuestion> Questions { get; set; } = new List<IQuestion>();
}
