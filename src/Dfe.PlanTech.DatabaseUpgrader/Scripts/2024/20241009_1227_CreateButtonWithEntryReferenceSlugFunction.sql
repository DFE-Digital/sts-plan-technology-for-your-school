CREATE OR ALTER VIEW [Contentful].[ButtonWithEntryReferencesWithSlug]
AS
  SELECT 
    buttons.Id, 
    buttons.ButtonId,
    buttons.LinkToEntryId,
    links.Slug,
    links.LinkType,
    components.Published,
    components.Archived,
    components.Deleted
  FROM 
    [Contentful].[ButtonWithEntryReferences] AS buttons
  JOIN 
      (
          SELECT Id, Slug, 'Question' AS LinkType FROM [Contentful].[Questions]
          UNION ALL
          SELECT Id, Slug, 'Page' AS LinkType FROM [Contentful].[Pages]
      ) AS links
  ON 
      buttons.LinkToEntryId = links.Id
  JOIN 
    [Contentful].[ContentComponents] AS components
  ON 
    buttons.Id = components.Id;
