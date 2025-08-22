using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Core.Helpers
{
    public static class ContentfulContentTypeHelper
    {
        public static string GetContentTypeName<TEntry>()
        {
            var typeName = typeof(TEntry).Name;
            if (ContentfulContentTypeConstants.EntryClassToContentTypeMap.TryGetValue(typeName, out var contentfulContentTypeId))
            {
                return contentfulContentTypeId!;
            }

            throw new InvalidOperationException($"Could not find content type ID for class type {typeName}");
        }
    }
}
