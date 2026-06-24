using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Validators.Interfaces;

public interface IGroupSelectSchoolsToAssessValidator
{
    Task ValidateSelectionAsync(
        GroupSelectSchoolsToAssessInputViewModel model,
        ModelStateDictionary modelState
    );
}

