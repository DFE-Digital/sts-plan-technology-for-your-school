BEGIN TRAN

--Establishment Table--
CREATE TABLE [dbo].[DataProtectionKeys](
	[Id] [int] PK IDENTITY(1,1) NOT NULL,
	[FriendlyName] [nvarchar](MAX) NULL,
	[Xml] [nvarchar](MAX) NOT NULL
)

COMMIT TRAN