using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Core.Exceptions;

/// <summary>
/// Used for when a question, answer, or some other key piece of user journey content is not found.
/// </summary>
/// <param name="message"></param>
public class UserJourneyMissingContentException(string message, ISectionComponent section) : Exception(message)
{
    public readonly ISectionComponent Section = section;
}
