// using Dfe.PlanTech.Application.Constants;
// using Dfe.PlanTech.Application.Exceptions;
// using Dfe.PlanTech.Domain.Establishments.Exceptions;
// using Dfe.PlanTech.Domain.SignIns.Enums;
// using Dfe.PlanTech.Domain.Users.Exceptions;
// using Microsoft.AspNetCore.Diagnostics;
// using Dfe.PlanTech.Domain.Exceptions;
// using Dfe.PlanTech.Domain.Submissions.Interfaces;
// using Microsoft.AspNetCore.Mvc.ViewFeatures;

// namespace Dfe.PlanTech.Web.Middleware;

// public class UserJourneyMissingContentExceptionHandler
// {
//   private readonly IDeleteCurrentSubmissionCommand _deleteCurrentSubmissionCommand;

//   public UserJourneyMissingContentExceptionHandler(IDeleteCurrentSubmissionCommand deleteCurrentSubmissionCommand)
//   {
//     _deleteCurrentSubmissionCommand = deleteCurrentSubmissionCommand;
//   }

//   public async Task<bool> TryHandleException(HttpContext context, Exception? exception, CancellationToken cancellationToken)
//   {
//     if (exception is null) return false;

//     if (exception is UserJourneyMissingContentException missingContentException)
//     {
//       await HandleException(context, missingContentException, cancellationToken);
//       return true;
//     }

//     return false;
//   }

//   private async Task HandleException(HttpContext context, UserJourneyMissingContentException exception, CancellationToken cancellationToken)
//   {
//     await _deleteCurrentSubmissionCommand.DeleteCurrentSubmission(exception.Section, cancellationToken);

//     SetRedirectUrl(context);
//   }

//   private void SetRedirectUrl(HttpContext context)
//   {
//     var tempData = GetTempData(context);

//     var configuration = GetService<IConfiguration>(context);

//     tempData["SubtopicError"] = configuration["ErrorMessages:ConcurrentUsersOrContentChange"];
//     context.Response.Redirect("/self-assessment");
//   }

//   private ITempDataDictionary GetTempData(HttpContext context)
//   {
//     var factory = GetService<ITempDataDictionaryFactory>(context);
//     return factory.GetTempData(context);
//   }

//   private T GetService<T>(HttpContext context) => context.RequestServices.GetRequiredService<T>();
// }
