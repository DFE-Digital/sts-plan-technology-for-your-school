using Dfe.PlanTech.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Handlers;

/// <summary>
/// Handles <see cref="UserJourneyMissingContentException"/> exceptions by deleting the broken submission
/// </summary>
public interface IUserJourneyMissingContentExceptionHandler
{
    /// <summary>
    /// Deletes the submission, then redirects the user back to the homepage with an appropriate error message. 
    /// </summary>
    /// <param name="controller">Controller that threw the exception</param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IActionResult> Handle(Controller controller, UserJourneyMissingContentException exception);
}
