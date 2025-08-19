using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.ViewModels;

public class GroupsCustomRecommendationIntroViewModel : IHeaderWithContent
{
    public List<ContentfulEntry> Content { get; init; } = [];
    public string HeaderText { get; init; } = null!;
    public string IntroContent { get; init; } = null!;
    public string LinkText { get; init; } = null!;
    public List<QuestionWithAnswerModel>? Responses { get; init; }
    public string SelectedEstablishmentName { get; init; } = null!;
    public string SlugifiedLinkText => LinkText.Slugify();
}
