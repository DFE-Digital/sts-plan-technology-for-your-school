using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/check-answers")]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    public CheckAnswersController(ILogger<CheckAnswersController> logger, IUrlHistory history) : base(logger, history) { }

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
    public IActionResult ConfirmCheckAnswers()
    {
        throw new NotImplementedException();
    }
}