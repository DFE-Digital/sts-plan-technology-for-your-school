BEGIN TRANSACTION;

  DELETE FROM [dbo].[response];
  DELETE FROM [dbo].[submission];
  DELETE FROM [dbo].[question];
  DELETE FROM [dbo].[answer];
  DELETE FROM [dbo].[signin];
  DELETE FROM [dbo].[user];
  DELETE FROM [dbo].[establishment];

COMMIT;
