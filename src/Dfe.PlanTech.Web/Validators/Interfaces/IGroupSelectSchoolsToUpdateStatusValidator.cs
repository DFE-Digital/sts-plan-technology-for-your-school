using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Validators.Interfaces;

public interface IGroupSelectSchoolsToUpdateStatusValidator
{
    Task ValidateSelectionAsync(
        GroupsSelectSchoolsToUpdateStatusViewModel model,
        ModelStateDictionary modelState
    );
}

