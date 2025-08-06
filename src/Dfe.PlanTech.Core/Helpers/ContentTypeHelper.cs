using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Core.Helpers
{
    public static class ContentTypeHelper
    {
        public static string GetContentTypeName<TEntry>()
        {
            var typeName = typeof(TEntry).Name;
            if (ContentTypeConstants.EntryClassToContentTypeMap.TryGetValue(typeName, out var contentTypeId))
            {
                return contentTypeId!;
            }

            throw new InvalidOperationException($"Could not find content type ID for class type {typeName}");
        }
    }
}
