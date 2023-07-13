using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/check-answers")]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly ICalculateMaturityCommand _calculateMaturityCommand;

    public CheckAnswersController(ILogger<CheckAnswersController> logger, IUrlHistory history,
                                  [FromServices] ICalculateMaturityCommand calculateMaturityCommand) : base(logger, history) 
    {
        _calculateMaturityCommand = calculateMaturityCommand;
    }

    [HttpGet]
    public IActionResult CheckAnswersPage()
    {
        Question[] questions = Array.Empty<Question>();

        CheckYourAnswersViewModel checkYourAnswersViewModel = new CheckYourAnswersViewModel()
        {
            Questions = questions,
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment"
        };

        return View("CheckYourAnswers", checkYourAnswersViewModel);
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId)
    {
        var calculateMaturity = await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);
        return null;
    }
}