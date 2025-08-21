namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetCategoryQuery : ContentRetriever, IGetCategoryQuery
{
    public const string SlugFieldPath = "fields.landingPage.fields.slug";

    public GetCategoryQuery(IContentRepository repository) : base(repository)
    {
    }

    public async Task<Category?> GetCategoryBySlug(string categorySlug, CancellationToken cancellationToken = default)
    {
        var options = new GetEntitiesOptions()
        {
            Include = 5,
            Queries =
            [
                new ContentQueryEquals()
                {
                    Field = SlugFieldPath,
                    Value = categorySlug
                },
                new ContentQueryEquals()
                {
                    Field = "fields.landingPage.sys.contentType.sys.id",
                    Value = "page"
                }
            ]
        };

        try
        {
            var categories = await repository.GetEntities<Category>(options, cancellationToken);
            return categories.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting category with slug {categorySlug} from Contentful", ex);
        }
    }
}
