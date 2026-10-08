/*
Change the column types of gias.establishmentGroup.ukprn and
gias.establishment.ukprn from INT to NVARCHAR(10).

Expect this to take ~30 seconds.
*/

/* gias.establishmentGroup.ukprn first */

-- 1. Add the new column alongside the old one
ALTER TABLE [gias].[establishmentGroup]
ADD [ukprn_new] NVARCHAR(10) NULL;
GO

-- 2. Drop the dependent index
DROP INDEX [IX_giasEstablishmentGroup_ukprn] ON [gias].[establishmentGroup];
GO

-- 3. Backfill in batches to avoid a long-running blocking transaction
DECLARE @BatchSize INT = 5000;
WHILE 1 = 1
BEGIN
    UPDATE TOP (@BatchSize) [gias].[establishmentGroup]
    SET [ukprn_new] = CAST([ukprn] AS NVARCHAR(10))
    WHERE [ukprn_new] IS NULL AND [ukprn] IS NOT NULL;

    IF @@ROWCOUNT = 0 BREAK;
END
GO

-- 4. Drop the old int column
ALTER TABLE [gias].[establishmentGroup] DROP COLUMN [ukprn];
GO

-- 5. Swap the columns. This needs a brief schema modification lock,
--    but it's just a drop and rename (not a full rewrite) so it's fast.
EXEC sp_rename '[gias].[establishmentGroup].[ukprn_new]', 'ukprn', 'COLUMN';
GO

-- 6. Recreate the index, preserving the filter
CREATE NONCLUSTERED INDEX [IX_giasEstablishmentGroup_ukprn]
ON [gias].[establishmentGroup] ([ukprn])
WHERE ([ukprn] IS NOT NULL);
GO

/* Now gias.establishment.ukprn */

-- 1. Add the new column alongside the old one
ALTER TABLE [gias].[establishment]
ADD [ukprn_new] NVARCHAR(10) NULL;
GO

-- 2. Drop the dependent index
DROP INDEX [IX_giasEstablishment_ukprn] ON [gias].[establishment];
GO

-- 3. Backfill in batches to avoid a long-running blocking transaction
DECLARE @BatchSize INT = 5000;
WHILE 1 = 1
BEGIN
    UPDATE TOP (@BatchSize) [gias].[establishment]
    SET [ukprn_new] = CAST([ukprn] AS NVARCHAR(10))
    WHERE [ukprn_new] IS NULL AND [ukprn] IS NOT NULL;

    IF @@ROWCOUNT = 0 BREAK;
END
GO

-- 4. Drop the old int column
ALTER TABLE [gias].[establishment] DROP COLUMN [ukprn];
GO

-- 5. Swap the columns. This needs a brief schema modification lock,
--    but it's just a drop and rename (not a full rewrite) so it's fast.
EXEC sp_rename '[gias].[establishment].[ukprn_new]', 'ukprn', 'COLUMN';
GO

-- 6. Recreate the index, preserving the filter
CREATE NONCLUSTERED INDEX [IX_giasEstablishment_ukprn]
ON [gias].[establishment] ([ukprn])
WHERE ([ukprn] IS NOT NULL);
GO
