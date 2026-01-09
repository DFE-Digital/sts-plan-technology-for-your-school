BEGIN TRANSACTION;

  CREATE TABLE [Contentful].[RecommendationChunks](
    [Id] NVARCHAR(30) NOT NULL,
    [HeaderId] NVARCHAR(30) NULL,
    [Title] NVARCHAR(150) NOT NULL,
    CONSTRAINT [PK_RecommendationChunks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecommendationChunks_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecommendationChunks_Headers_HeaderId] FOREIGN KEY ([HeaderId]) REFERENCES [Contentful].[Headers] ([Id])
  );

  CREATE TABLE [Contentful].[RecommendationChunkContents] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationChunkId] NVARCHAR(30) NULL,
      [ContentComponentId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationChunkContents_RecommendationChunkId] FOREIGN KEY ([RecommendationChunkId]) REFERENCES [Contentful].[RecommendationChunks] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationChunkContents_ContentComponentId] FOREIGN KEY ([ContentComponentId]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE NO ACTION,
  );

  CREATE TABLE [Contentful].[RecommendationChunkAnswers] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationChunkId] NVARCHAR(30) NULL,
      [AnswerId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationChunkAnswers_RecommendationChunkId] FOREIGN KEY ([RecommendationChunkId]) REFERENCES [Contentful].[RecommendationChunks] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationChunkAnswers_AnswerId] FOREIGN KEY ([AnswerId]) REFERENCES [Contentful].[Answers] ([Id]) ON DELETE NO ACTION,
  );

  CREATE TABLE [Contentful].[RecommendationIntros](
    [Id] NVARCHAR(30) NOT NULL,
    [HeaderId] NVARCHAR(30) NULL,
    [Maturity] NVARCHAR(40) NOT NULL,
    CONSTRAINT [PK_RecommendationIntros] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecommendationIntros_Headers_HeaderId] FOREIGN KEY ([HeaderId]) REFERENCES [Contentful].[Headers] ([Id])
  );

  CREATE TABLE [Contentful].[RecommendationIntroContents] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationIntroId] NVARCHAR(30) NULL,
      [ContentComponentId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationIntroContents_RecommendationIntroId] FOREIGN KEY ([RecommendationIntroId]) REFERENCES [Contentful].[RecommendationIntros] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationIntroContents_ContentComponentId] FOREIGN KEY ([ContentComponentId]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE NO ACTION,
  );

  CREATE TABLE [Contentful].[RecommendationSections](
    [Id] NVARCHAR(30) NOT NULL,
    CONSTRAINT [PK_RecommendationSections] PRIMARY KEY ([Id]),
  );

  CREATE TABLE [Contentful].[RecommendationSectionChunks] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationSectionId] NVARCHAR(30) NULL,
      [RecommendationChunkId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationSectionContents_RecommendationSectionId] FOREIGN KEY ([RecommendationSectionId]) REFERENCES [Contentful].[RecommendationSections] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationSectionContents_RecommendationChunkId] FOREIGN KEY ([RecommendationChunkId]) REFERENCES [Contentful].[RecommendationChunks] ([Id]) ON DELETE NO ACTION,
  );

  CREATE TABLE [Contentful].[RecommendationSectionAnswers] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationSectionId] NVARCHAR(30) NULL,
      [AnswerId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationSectionAnswers_RecommendationSectionId] FOREIGN KEY ([RecommendationSectionId]) REFERENCES [Contentful].[RecommendationSections] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationSectionAnswers_AnswerId] FOREIGN KEY ([AnswerId]) REFERENCES [Contentful].[Answers] ([Id]) ON DELETE NO ACTION,
  );

  CREATE TABLE [Contentful].[SubtopicRecommendations](
    [Id] NVARCHAR(30) NOT NULL,
    [SectionId] NVARCHAR(30) NULL,
    [SubtopicId] NVARCHAR(30) NULL,
    CONSTRAINT [PK_SubtopicRecommendations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubtopicRecommendations_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Contentful].[RecommendationSections] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubtopicRecommendations_SubtopicId] FOREIGN KEY ([SubtopicId]) REFERENCES [Contentful].[Sections] ([Id]) ON DELETE NO ACTION,
  );


  CREATE TABLE [Contentful].[SubtopicRecommendationIntros](
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [SubtopicRecommendationId] NVARCHAR(30) NULL,
      [RecommendationIntroId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_SubtopicRecommendationIntros_SubtopicRecommendationId] FOREIGN KEY ([SubtopicRecommendationId]) REFERENCES [Contentful].[SubtopicRecommendations] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_SubtopicRecommendationIntros_RecommendationIntroId] FOREIGN KEY ([RecommendationIntroId]) REFERENCES [Contentful].[RecommendationIntros] ([Id]) ON DELETE NO ACTION,
  );

COMMIT;
