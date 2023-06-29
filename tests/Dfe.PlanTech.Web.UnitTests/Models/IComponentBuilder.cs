﻿using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public interface IComponentBuilder
    {
        ButtonWithLink BuildButtonWithLink();
        Category BuildCategory();
        ComponentDropDown BuildDropDownComponent();
        TextBody BuildTextBody();
    }
}