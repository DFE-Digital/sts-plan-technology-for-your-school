using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class WarningComponentTests
    {
        [Fact]
        public void WarningComponent_Sets_Text()
        {
            //Arrange/Act
            var warningComponent = new WarningComponent
            {
                Text = new TextBody
                {
                    RichText = new RichTextContent
                    {
                        Value = "test"
                    }
                }
            };

            //Assert
            Assert.Equal("test", warningComponent.Text.RichText.Value);

        }
    }
}
