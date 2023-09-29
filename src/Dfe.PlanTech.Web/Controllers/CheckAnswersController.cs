using Dfe.PlanTech.Domain.Submissions.Interfaces;
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
                                                      [FromServices] ICheckAnswersRouter checkAnswersValidator,
                                                      CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        return await checkAnswersValidator.ValidateRoute(sectionSlug, this, cancellationToken);
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ICalculateMaturityCommand calculateMaturityCommand, CancellationToken cancellationToken = default)
    {
        if (submissionId <= 0) throw new ArgumentOutOfRangeException(nameof(submissionId));
        if (string.IsNullOrEmpty(sectionName)) throw new ArgumentNullException(nameof(sectionName));
        
        await calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);

        TempData["SectionName"] = sectionName;

        return this.RedirectToSelfAssessment();
    }
}