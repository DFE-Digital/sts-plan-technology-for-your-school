using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsSubtopicRecommendationDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public List<CmsRecommendationIntroDto> Intros { get; set; } = [];
        public CmsRecommendationSectionDto Section { get; set; } = null!;
        public CsmQuestionnaireSectionDto Subtopic { get; set; } = null!;

        public CmsSubtopicRecommendationDto(SubtopicRecommendationEntry subtopicRecommendationEntry)
        {
            Id = subtopicRecommendationEntry.Id;
            InternalName = subtopicRecommendationEntry.InternalName;
            Intros = subtopicRecommendationEntry.Intros.Select(i => i.AsDto()).ToList();
        }
    }
}
