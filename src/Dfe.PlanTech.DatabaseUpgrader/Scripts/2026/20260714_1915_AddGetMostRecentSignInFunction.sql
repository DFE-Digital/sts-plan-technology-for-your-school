/****** Object:  UserDefinedFunction [dbo].[getMatEstablishmentIdForUserIdEstablishmentIdPaidOnGivenDate]    Script Date: 15/07/2026 11:41:44 ******/
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
CREATE FUNCTION [dbo].[getMatEstablishmentIdForUserIdEstablishmentIdOnGivenDate]
(
	@userId INT,
	@establishmentId INT,
	@dateTime SMALLDATETIME
)
RETURNS INT
AS
BEGIN
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

    -- Return the result of the function
    RETURN @matEstablishmentId
END
GO
