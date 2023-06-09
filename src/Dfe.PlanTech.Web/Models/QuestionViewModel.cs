using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel
{
    public required Question Question { get; init; }
    
    public required string BackUrl { get; init;}
}
