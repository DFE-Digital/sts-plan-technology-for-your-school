CREATE TABLE [dbo].[userContentView](
	[id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED ([id] ASC),
	[userId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[user](id),
	[contentfulRef] NVARCHAR(50) NOT NULL,
	[userActionId] [uniqueidentifier] NOT NULL
);

CREATE INDEX [IX_UserContentViews_ContentfulRef] ON [dbo].[userContentView] ([contentfulRef]);
GO
