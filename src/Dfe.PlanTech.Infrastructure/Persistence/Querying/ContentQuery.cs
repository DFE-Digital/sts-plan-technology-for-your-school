using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfe.PlanTech.Infrastructure.Persistence.Querying;

namespace Dfe.PlanTech.Infrastructure.Persistence.Querying
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