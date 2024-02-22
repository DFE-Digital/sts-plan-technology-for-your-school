BEGIN TRANSACTION;
GO;

  CREATE TABLE [Contentful].[RecommendationChunks](
    [Id] NVARCHAR(30) NOT NULL,
    [HeaderId] nvarchar(30) NULL,
    CONSTRAINT [PK_RecommendationChunks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecommendationChunks_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecommendationChunks_Headers_HeaderId] FOREIGN KEY ([HeaderId]) REFERENCES [Contentful].[Headers] ([Id])
  );
  GO;

  CREATE TABLE [Contentful].[RecommendationChunkContents] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationChunkId] NVARCHAR(30) NULL,
      [ContentComponentId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationChunkContents_RecommendationChunkId] FOREIGN KEY ([RecommendationChunkId]) REFERENCES [Contentful].[RecommendationChunks] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationChunkContents_ContentComponentId] FOREIGN KEY ([ContentComponentId]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE NO ACTION,
  );
  GO;


  CREATE TABLE [Contentful].[RecommendationChunkAnswers] (
      [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
      [RecommendationChunkId] NVARCHAR(30) NULL,
      [AnswerId] NVARCHAR(30) NULL,
      CONSTRAINT [FK_RecommendationChunkContents_RecommendationChunkId] FOREIGN KEY ([RecommendationChunkId]) REFERENCES [Contentful].[RecommendationChunks] ([Id]) ON DELETE NO ACTION,
      CONSTRAINT [FK_RecommendationChunkContents_AnswerId] FOREIGN KEY ([AnswerId]) REFERENCES [Contentful].[Answers] ([Id]) ON DELETE NO ACTION,
  );
  GO;

COMMIT;
GO;

