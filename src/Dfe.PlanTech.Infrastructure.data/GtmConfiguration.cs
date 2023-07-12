using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Infrastructure.Data
{
    public class GtmConfiguration : IGtmConfiguration
    {
        public string Head { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
