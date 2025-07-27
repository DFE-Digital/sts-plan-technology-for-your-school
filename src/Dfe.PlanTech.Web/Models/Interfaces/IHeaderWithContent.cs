namespace Dfe.PlanTech.Core.Contentful.Models.Interfaces
{
    public interface IHeaderWithContent
    {
        public string HeaderText { get; }

        public List<ContentComponent> Content { get; }

        public string LinkText { get; }

        public string SlugifiedLinkText { get; }
    }
}
