ALTER PROCEDURE GetSectionStatuses
    @splitStringVal nvarchar(255),
    @establishmentId int
    AS
DECLARE @val NVARCHAR(50)
CREATE TABLE #splitTable(val nvarchar(50))
CREATE TABLE #ReturnTable(sectionId nvarchar(50), completed int, maturity nvarchar(20), dateCreated datetime)

    INSERT INTO #splitTable
SELECT * FROM STRING_SPLIT(@splitStringVal, ',')

DECLARE db_cursor CURSOR FOR
SELECT val FROM #splitTable

    OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @val

    WHILE @@FETCH_STATUS = 0
BEGIN
INSERT INTO #ReturnTable
select Top 1 sectionId, completed, maturity, dateCreated from submission
where sectionId = @val
and establishmentId = @establishmentId
order by dateCreated desc
    FETCH NEXT FROM db_cursor INTO @val
END

CLOSE db_cursor
    DEALLOCATE db_cursor

select * from #ReturnTable
