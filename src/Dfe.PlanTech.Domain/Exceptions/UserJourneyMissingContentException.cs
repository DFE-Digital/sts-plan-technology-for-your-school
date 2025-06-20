using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Exceptions;

/// <summary>
/// Used for when a question, answer, or some other key piece of user journey content is not found.
/// </summary>
/// <param name="message"></param>
public class UserJourneyMissingContentException(string message, ISectionComponent section) : Exception(message)
{
    public readonly ISectionComponent Section = section;
}
