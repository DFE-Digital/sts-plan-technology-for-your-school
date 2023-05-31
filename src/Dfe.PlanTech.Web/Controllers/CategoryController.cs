using Microsoft.AspNetCore.Mvc;
using Dfe.PlanTech.Application.Questionnaire.Queries;

namespace Dfe.PlanTech.Web.Controllers
{
    /*
    [ApiController]
    [Route("api/{controller}")]
    public class CategoryController
    {
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ILogger<CategoryController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromServices] GetCategoriesQuery query)
        {
            try
            {
                var categories = await query.Get();

                return new OkObjectResult(categories);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Error fetching categories");
                return new BadRequestObjectResult(new { Error = "Error fetching categories" });
            }
        }
    }
    */
}