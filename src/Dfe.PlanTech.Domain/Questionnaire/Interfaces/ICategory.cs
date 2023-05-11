namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory
{
    string Name { get; }
    
    string Title { get; }
    
    List<IQuestion> Questions { get; }
}
