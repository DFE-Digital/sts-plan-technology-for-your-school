/*
This is an example script that shows the stored proc script naming convention.

To help with maintainability stick to 1 stored procedure per file 

Ensure the stored proc is declared with "CREATE OR ALTER" as stored procs are ALWAYS deployed.

file format is SP_<description>.sql

e.g. SP_ReadUser.sql


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [sp_readuser]
    -- Add the parameters for the stored procedure here
    @Param1 INT,
    @Param2 NVARCHAR(50)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    -- Insert statements for procedure here
    
END
GO

*/