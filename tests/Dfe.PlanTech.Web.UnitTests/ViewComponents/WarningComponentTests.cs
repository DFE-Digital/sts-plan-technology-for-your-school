using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.UnitTests.Models;
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
        public string testText = "test text";
        public string? nullText = null;
        public string emptyText = "";
        public ComponentBuilder componentBuilder = new ComponentBuilder();

        [Fact]
        public void WarningComponent_Sets_Text()
        {
            var testWarning = componentBuilder.BuildWarningComponent(testText);
            Assert.Equal("test text", testWarning.Text.RichText.Value);
        }

        [Fact]
        public void WarningComponent_Sets_Empty_String()
        {
            var testWarning = componentBuilder.BuildWarningComponent(emptyText);
            Assert.Equal("", testWarning.Text.RichText.Value);
        }

        [Fact]
        public void WarningComponent_Sets_Null()
        {
            var testWarning = componentBuilder.BuildWarningComponent(nullText!);
            Assert.Null(testWarning.Text.RichText.Value);
        }
    }
}
