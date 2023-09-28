using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    public const string CheckAnswersPageSlug = "check-answers";
    public const string CheckAnswersViewName = "CheckAnswers";

    public CheckAnswersController(ILogger<CheckAnswersController> logger) : base(logger)
    {
    }

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionSlug,
                                                      [FromServices] CheckAnswersValidator checkAnswersValidator,
                                                      CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        return await checkAnswersValidator.ValidateRoute(sectionSlug, this, cancellationToken);
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ICalculateMaturityCommand calculateMaturityCommand, CancellationToken cancellationToken = default)
    {
        await calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);

        TempData["SectionName"] = sectionName;

        return this.RedirectToSelfAssessment();
    }
}