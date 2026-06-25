using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Validators.Interfaces;

public interface IGroupSelectSchoolsToAssessValidator
{
    Task ValidateSelectionAsync(
        GroupsSelectSchoolsToAssessViewModel model,
        ModelStateDictionary modelState
    );
}

