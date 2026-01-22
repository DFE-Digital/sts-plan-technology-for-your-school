BEGIN TRAN

--Establishment Table--
CREATE TABLE [dbo].[establishment](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[establishmentRef] [nvarchar](50) NOT NULL,
	[establishmentType] [nvarchar](50) NOT NULL,
	[orgName] [nvarchar](50) NOT NULL,
	[dateCreated] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
	[dateLastUpdated] [datetime] NULL,
 CONSTRAINT [PK_establishment1] PRIMARY KEY CLUSTERED ( [id] ASC)
)

--User Table--
CREATE TABLE [dbo].[user](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[dfeSignInRef] [nvarchar](30) NOT NULL,
	[dateCreated] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
	[dateLastUpdated] [datetime] NULL,
 CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED ([id] ASC)
)

--SignIn Table--
CREATE TABLE [dbo].[signIn](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [int] NOT NULL,
	[establishmentId] [int] NOT NULL,
	[signInDateTime] [timestamp] NOT NULL,
 CONSTRAINT [PK_signIn] PRIMARY KEY CLUSTERED ( [id] ASC)
)

ALTER TABLE [dbo].[signIn] WITH CHECK
ADD CONSTRAINT [FK_establishmentId_establishmentId] FOREIGN KEY([establishmentId])
REFERENCES [dbo].[establishment] ([id])

ALTER TABLE [dbo].[signIn] CHECK CONSTRAINT [FK_establishmentId_establishmentId]

ALTER TABLE [dbo].[signIn]  WITH CHECK ADD  CONSTRAINT [FK_userId_id] FOREIGN KEY([userId])
REFERENCES [dbo].[user] ([id])

ALTER TABLE [dbo].[signIn] CHECK CONSTRAINT [FK_userId_id]

--Submission Table--
CREATE TABLE [dbo].[submission](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[establishmentId] [int] NOT NULL,
	[completed] [bit] NOT NULL,
	[sectionId] [int] NOT NULL,
	[sectionName] [nvarchar](50) NULL,
	[maturity] [nvarchar](20) NULL,
	[recomendationId] [int] NULL,
	[dateCreated] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
	[dateLastUpdated] [datetime] NULL,
	[dateCompleted] [datetime] NULL,
 CONSTRAINT [PK_submission] PRIMARY KEY CLUSTERED ([id] ASC)
)

ALTER TABLE [dbo].[submission]  WITH CHECK ADD  CONSTRAINT [FK_submission_establishment] FOREIGN KEY([establishmentId])
REFERENCES [dbo].[establishment] ([id])

ALTER TABLE [dbo].[submission] CHECK CONSTRAINT [FK_submission_establishment]


--Question Table--
CREATE TABLE [dbo].[question](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[questionText] [nvarchar](max) NULL,
	[contentfulRef] [nvarchar](50) NOT NULL,
	[dateCreated] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
 CONSTRAINT [PK_question] PRIMARY KEY CLUSTERED ([id] ASC)
)

--Answer TABLE--
CREATE TABLE [dbo].[answer](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[answerText] [nvarchar](max) NOT NULL,
	[contentfulRef] [nvarchar](50) NOT NULL,
	[dateCreated] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
 CONSTRAINT [PK_answer] PRIMARY KEY CLUSTERED ([id] ASC)
)

--Response Table--
CREATE TABLE [dbo].[response](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [int] NOT NULL,
	[submissionId] [int] NOT NULL,
	[questionId] [int] NOT NULL,
	[answerId] [int] NOT NULL,
	[maturity] [nvarchar](20) NOT NULL,
	[dateCreated] [datetime] NOT NULL DEFAULT (GETUTCDATE()),
	[dateLastUpdated] [datetime] NULL,
 CONSTRAINT [PK_response] PRIMARY KEY CLUSTERED ([id] ASC)
)

ALTER TABLE [dbo].[response]  WITH CHECK ADD  CONSTRAINT [FK_answerId_answer] FOREIGN KEY([answerId])
REFERENCES [dbo].[answer] ([id])

ALTER TABLE [dbo].[response] CHECK CONSTRAINT [FK_answerId_answer]

ALTER TABLE [dbo].[response]  WITH CHECK ADD  CONSTRAINT [FK_questionId_Question] FOREIGN KEY([questionId])
REFERENCES [dbo].[question] ([id])

ALTER TABLE [dbo].[response] CHECK CONSTRAINT [FK_questionId_Question]

ALTER TABLE [dbo].[response]  WITH CHECK ADD  CONSTRAINT [FK_submissionId_submission] FOREIGN KEY([submissionId])
REFERENCES [dbo].[submission] ([id])

ALTER TABLE [dbo].[response] CHECK CONSTRAINT [FK_submissionId_submission]

ALTER TABLE [dbo].[response]  WITH CHECK ADD  CONSTRAINT [FK_userId_User] FOREIGN KEY([userId])
REFERENCES [dbo].[user] ([id])

ALTER TABLE [dbo].[response] CHECK CONSTRAINT [FK_userId_User]


--ROLLBACK TRAN
COMMIT TRAN
