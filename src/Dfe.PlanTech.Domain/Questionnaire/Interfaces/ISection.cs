using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

/// <summary>
/// A subsection of a <see chref="ICategory"/>
/// </summary>
public interface ISection
{
    public string Name { get; }
}

/// <summary>
/// A subsection of a <see chref="ICategory"/>
/// </summary>
public interface ISection<TQuestion, out TPage> : ISection
where TQuestion : IQuestion
where TPage : IPage
{
    public List<TQuestion> Questions { get; }

    public TPage? InterstitialPage { get; }
}

/// <summary>
/// Interface for the <see cref="ISection"/> for the Contentful data 
/// </summary>
public interface ISectionComponent : ISection<Question, Page>, IContentComponent
{
    public string FirstQuestionId { get; }


    /// <summary>
    /// Puts responses into the correct order for the path the user has taken.
    /// </summary>
    /// <returns>Answered questions in correct journey order</returns>
    public IEnumerable<QuestionWithAnswer> GetOrderedResponsesForJourney(IEnumerable<QuestionWithAnswer> responses);
}
