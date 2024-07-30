using System.Reflection;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator;

public class ComparatorFactory
{
    private readonly Type[] _comparatorTypes;

    private readonly Type _baseComparatorType = typeof(BaseComparator);
    private readonly Type _dbType = typeof(CmsDbContext);
    private readonly Type _contentfulContentType = typeof(ContentfulContent);
    public readonly List<BaseComparator> Comparators;

    public ComparatorFactory(CmsDbContext db, ContentfulContent contentfulContent)
    {
        _comparatorTypes = Assembly.GetExecutingAssembly().GetTypes()
                                .Where(type => !type.IsAbstract && type.IsSubclassOf(_baseComparatorType))
                                .ToArray();


        Comparators = _comparatorTypes.Select(type =>
        {
            var constructor = type.GetConstructor([_dbType, _contentfulContentType]);
            return (BaseComparator)constructor!.Invoke([db, contentfulContent]);
        }).ToList();
    }
}
