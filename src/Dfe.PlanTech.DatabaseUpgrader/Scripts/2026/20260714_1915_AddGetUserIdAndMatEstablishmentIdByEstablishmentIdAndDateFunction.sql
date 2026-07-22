/****** Object:  UserDefinedFunction [dbo].[getMatEstablishmentIdForUserIdEstablishmentIdOnGivenDate]    Script Date: 15/07/2026 14:51:27 ******/
DROP FUNCTION [dbo].[getUserIdAndMatEstablishmentIdByEstablishmentIdAndDate]
GO

/****** Object:  UserDefinedFunction [dbo].[getMatEstablishmentIdForUserIdEstablishmentIdOnGivenDate]    Script Date: 15/07/2026 14:51:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Drew Morgan
-- Create date: 2026-07-14
-- Description:	Function to get userId, establishmentId, and matEstablishmentId
--              of most recent sign-in or groupReadActivity
-- =============================================
CREATE FUNCTION [dbo].[getUserIdAndMatEstablishmentIdByEstablishmentIdAndDate]
(
	@establishmentId INT,
	@dateTime SMALLDATETIME
)
RETURNS
@output TABLE
(
	userId INT,
	matEstablishmentId INT
)
AS
BEGIN

	DECLARE @userId INT

	SELECT TOP 1
		@userId = userId
	FROM
		dbo.signIn si
	WHERE
		si.establishmentId = @establishmentId AND
		si.signInDateTime BETWEEN DATEADD(DAY, -1, @dateTime) AND @dateTime
	ORDER BY
		si.signInDateTime DESC

	DECLARE @matEstablishmentId INT

	SELECT TOP 1
		@matEstablishmentId = establishmentId
	FROM
		dbo.groupReadActivity gra1
	WHERE
		gra1.userId = @userId AND
		gra1.selectedEstablishmentId = @establishmentId AND
		gra1.dateSelected BETWEEN DATEADD(DAY, -1, @dateTime) AND @dateTime
	ORDER BY
		gra1.dateSelected

	INSERT INTO @output	SELECT @userId, @matEstablishmentId

	RETURN
END
GO
