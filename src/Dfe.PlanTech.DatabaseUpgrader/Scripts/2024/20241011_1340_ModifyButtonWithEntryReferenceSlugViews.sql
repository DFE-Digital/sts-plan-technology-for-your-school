DELETE VIEW [Contentful].[ButtonWithEntryReferencesWithSlug]

CREATE OR ALTER VIEW [Contentful].[SlugsForButtonWithEntryReferences]
AS
  SELECT 
    buttons.Id, 
    links.Slug,
    links.LinkType
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
