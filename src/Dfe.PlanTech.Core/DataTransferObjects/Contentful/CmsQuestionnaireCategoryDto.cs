using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsQuestionnaireCategoryDto : CmsEntryDto
{
    public CmsQuestionnaireCategoryDto(QuestionnaireCategoryEntry categoryEntry)
    {
        Id = categoryEntry.Id;
        InternalName = categoryEntry.InternalName;
        Header = categoryEntry.Header.AsDto();
        Content = categoryEntry.Content?.Select(BuildContentDto).ToList();
        Sections = categoryEntry.Sections.Select(s => s.AsDto()).ToList();

        //*
        LandingPage = categoryEntry.LandingPage?.AsDto();
        /*/
        categoryEntry.LandingPage = categoryEntry.LandingPage is null
            ? null
            : new PageEntry // Create new PageEntry without Content property to prevent looping
            {
                InternalName = categoryEntry.LandingPage.InternalName ?? string.Empty,
                Slug = categoryEntry.LandingPage.Slug,
                DisplayHomeButton = categoryEntry.LandingPage.DisplayHomeButton,
                DisplayBackButton = categoryEntry.LandingPage.DisplayBackButton,
                DisplayOrganisationName = categoryEntry.LandingPage.DisplayOrganisationName,
                DisplayTopicTitle = categoryEntry.LandingPage.DisplayTopicTitle,
                IsLandingPage = categoryEntry.LandingPage.IsLandingPage,
                RequiresAuthorisation = categoryEntry.LandingPage.RequiresAuthorisation,
                SectionTitle = categoryEntry.LandingPage.SectionTitle,
                BeforeTitleContent = categoryEntry.LandingPage.BeforeTitleContent,
                Title = categoryEntry.LandingPage.Title
            };

        LandingPage = categoryEntry.LandingPage?.AsDto();
        //*/
    }

    public string Id { get; set; }
    public string InternalName { get; set; } = "";
    public CmsComponentHeaderDto Header { get; set; } = null!;
    public List<CmsEntryDto>? Content { get; set; } = null!;
    public CmsPageDto? LandingPage { get; set; } = null!;
    public List<CmsQuestionnaireSectionDto> Sections { get; set; } = [];
    public List<SqlSectionStatusDto> SectionStatuses { get; set; } = [];
}
