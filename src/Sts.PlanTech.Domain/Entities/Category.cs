using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Sts.PlanTech.Domain.Entities;

public class Category
{
    public string Name { get; set; } = null!;

    public string Title { get; set; } = null!;

    //This should be "Document" type from Contentful, as is Rich Text
    public object? Description { get; set; }

    public List<Question> Questions { get; set; } = new List<Question>();
}
