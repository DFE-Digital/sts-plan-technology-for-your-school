using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Exceptions;

/// <summary>
/// Used for when a question, answer, or some other key piece of user journey content is not found.
/// </summary>
/// <param name="message"></param>
public class UserJourneyMissingContentException(string message, QuestionnaireSectionEntry section)
    : Exception(message)
{
    public readonly QuestionnaireSectionEntry Section = section;
}
