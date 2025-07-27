using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Interfaces
{
    public interface IHeaderWithContent
    {
        public string HeaderText { get; }
        public List<CmsEntryDto> Content { get; }
        public string LinkText { get; }
        public string SlugifiedLinkText { get; }
    }
}
