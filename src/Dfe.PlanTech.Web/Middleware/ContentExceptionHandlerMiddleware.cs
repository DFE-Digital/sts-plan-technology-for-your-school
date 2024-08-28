using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Submissions.Interfaces;

namespace Dfe.PlanTech.Web.Middleware;

public class UserJourneyMissingContentExceptionHandler
{
  private readonly IDeleteCurrentSubmissionCommand _deleteCurrentSubmissionCommand;

  public UserJourneyMissingContentExceptionHandler(IDeleteCurrentSubmissionCommand deleteCurrentSubmissionCommand)
  {
    _deleteCurrentSubmissionCommand = deleteCurrentSubmissionCommand;
  }

  public async Task<bool> TryHandleException(Exception? exception, CancellationToken cancellationToken)
  {
    if (exception is null) return false;

    if (exception is UserJourneyMissingContentException missingContentException)
    {
      await HandleException(missingContentException, cancellationToken);
      return true;
    }

    return false;
  }

  private Task HandleException(UserJourneyMissingContentException exception, CancellationToken cancellationToken) => _deleteCurrentSubmissionCommand.DeleteCurrentSubmission(exception.Section, cancellationToken);
}
