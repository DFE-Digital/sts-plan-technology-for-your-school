using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CookieBannerControllerTests
    {
        public CookiesController CreateStrut()
        {
            return new CookiesController();
        }

        [Theory]
        [InlineData("https://localhost:8080/self-assessment")]
        [InlineData("https://www.dfe.gov.uk/self-assessment")]
        public void HideBanner_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = url;
            strut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
       //     var result = strut.HideBanner();

       //     Microsoft.AspNetCore.Mvc.RedirectResult
            //Assert
            // Assert.Equal(url, result.);
        }
    }
}
