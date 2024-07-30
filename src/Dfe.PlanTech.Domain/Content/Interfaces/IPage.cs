using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IPage
{
    public string InternalName { get; }

    public string Slug { get; }

    public bool DisplayBackButton { get; }

    public bool DisplayHomeButton { get; }

    public bool DisplayTopicTitle { get; }

    public bool DisplayOrganisationName { get; }

    public bool RequiresAuthorisation { get; }
}

public interface IPage<TContentComponent, TTitle> : IPage
where TTitle : class, ITitle
where TContentComponent : class, IContentComponentType
{
    public TTitle? Title { get; }

    public List<TContentComponent> BeforeTitleContent { get; }

    public List<TContentComponent> Content { get; }
}

public interface IPageContent : IPage<ContentComponent, Title>
{
    public string? SectionTitle { get; set; }

    public string? OrganisationName { get; set; }
}
