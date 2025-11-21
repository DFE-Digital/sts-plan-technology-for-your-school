CREATE TABLE [dbo].[userSettings](
	[userId] int PRIMARY KEY,
	[sortOrder] varchar(50) NULL,

	CONSTRAINT [FK_userSettings_userId_id] FOREIGN KEY([userId]) REFERENCES [dbo].[user] ([id]),
	CONSTRAINT [UQ_userId] UNIQUE(userId)
)
GO
