using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sts.PlanTech.Infrastructure.Persistence.Querying;

namespace Sts.PlanTech.Infrastructure.Persistence
{
    public class ContentQuery
    {
        public string Field { get; init; } = null!;
    }

    public class ContentQueryEquals : ContentQuery
    {
        public string Value { get; init; } = null!;
    }

    public class ContentQueryIncludes : ContentQuery
    {
        public IEnumerable<string> Value { get; init; } = null!;
    }
}