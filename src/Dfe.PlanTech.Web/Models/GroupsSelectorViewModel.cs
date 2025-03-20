﻿using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class GroupsSelectorViewModel
    {
        public string GroupName { get; set; } = null!;

        public List<EstablishmentLink> GroupEstablishments { get; set; } = null!;

        public Title Title { get; init; } = null!;

        public List<ContentComponent> Content { get; init; } = null!;

    }
}
