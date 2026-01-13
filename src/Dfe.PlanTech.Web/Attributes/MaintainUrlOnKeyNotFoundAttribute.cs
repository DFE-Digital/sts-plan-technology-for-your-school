using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class MaintainUrlOnKeyNotFoundAttribute(
    IOptions<ContactOptionsConfiguration> contactOptions,
    IContentfulService contentfulService
) : ExceptionFilterAttribute, IAsyncExceptionFilter
{
    private readonly ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly IContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));

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
            var contactLink = await _contentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
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
