using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Contentful.Interfaces
{
    public interface IHeaderWithContent
    {
        public string HeaderText { get; }
        public List<ContentfulEntry> Content { get; }
        public string LinkText { get; }
        public string SlugifiedLinkText { get; }
    }
}
