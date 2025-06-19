using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Attributes
{
    public class MaintainUrlOnKeyNotFoundAttribute : ExceptionFilterAttribute, IAsyncExceptionFilter
    {
        private readonly IGetNavigationQuery _getNavigationQuery;
        private readonly IOptions<ContactOptionsConfiguration> _contactOptions;

        public MaintainUrlOnKeyNotFoundAttribute(
            IGetNavigationQuery getNavigationQuery,
            IOptions<ContactOptionsConfiguration> contactOptions
        )
        {
            _getNavigationQuery = getNavigationQuery;
            _contactOptions = contactOptions;
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;

            var isContentfulDataUnavailableException = exception is ContentfulDataUnavailableException;

            var isKeyNotFoundException = exception is KeyNotFoundException;
            var isNonOrganisationKeyNotFoundException = isKeyNotFoundException &&
                !exception.Message.Contains(ClaimConstants.Organisation);

            if (isContentfulDataUnavailableException || isNonOrganisationKeyNotFoundException)
            {
                // Use the injected service to get the contact link.
                var contactLink = await _getNavigationQuery.GetLinkById(_contactOptions.Value.LinkId);
                var viewModel = new NotFoundViewModel
                {
                    ContactLinkHref = contactLink?.Href
                };

                // Build the ViewResult
                var viewResult = new ViewResult
                {
                    ViewName = "NotFoundError",
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(), context.ModelState)
                    {
                        Model = viewModel
                    }
                };

                context.Result = viewResult;
                context.ExceptionHandled = true;
            }

            await base.OnExceptionAsync(context);
        }
    }
}
