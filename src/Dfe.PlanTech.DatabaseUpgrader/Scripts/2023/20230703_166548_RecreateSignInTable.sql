BEGIN TRAN

	drop table dbo.signIn

	CREATE TABLE [dbo].[signIn](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [int] NOT NULL,
	[establishmentId] [int] NOT NULL,
	[signInDateTime] [datetime] NOT NULL DEFAULT (GETUTCDATE())
	 CONSTRAINT [PK_signIn] PRIMARY KEY CLUSTERED ( [id] ASC)
	)

	ALTER TABLE [dbo].[signIn] WITH CHECK
	ADD CONSTRAINT [FK_establishmentId_establishmentId] FOREIGN KEY([establishmentId])
	REFERENCES [dbo].[establishment] ([id])

	ALTER TABLE [dbo].[signIn] CHECK CONSTRAINT [FK_establishmentId_establishmentId]

	ALTER TABLE [dbo].[signIn]  WITH CHECK ADD  CONSTRAINT [FK_userId_id] FOREIGN KEY([userId])
	REFERENCES [dbo].[user] ([id])

COMMIT TRAN
