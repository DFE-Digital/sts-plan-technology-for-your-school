BEGIN TRAN

--Cache Table--
CREATE TABLE [dbo].[MemoryCache](
	[Id] [nvarchar](449) NOT NULL,
	[Value] [varbinary](MAX) NOT NULL,
	[ExpiresAtTime] [DATETIMEOFFSET](7) NOT NULL,
    [SlidingExpirationInSeconds] [bigint] NULL,
	[AbsoluteExpiration] [DATETIMEOFFSET](7) NULL,
)

COMMIT TRAN