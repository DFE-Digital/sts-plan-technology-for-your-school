BEGIN TRANSACTION;
GO;

  DELETE FROM [dbo].[response];
  GO;

  DELETE FROM [dbo].[submission];
  GO;

  DELETE FROM [dbo].[question];
  GO;

  DELETE FROM [dbo].[answer];
  GO;

  DELETE FROM [dbo].[signin];
  GO;

  DELETE FROM [dbo].[user];
  GO;

  DELETE FROM [dbo].[establishment];
  GO;

COMMIT;
GO;