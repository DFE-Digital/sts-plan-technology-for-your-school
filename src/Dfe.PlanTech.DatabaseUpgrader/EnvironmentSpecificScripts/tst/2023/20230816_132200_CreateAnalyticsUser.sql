-- $Analytics_Username$ $Analytics_Password$

CREATE USER [$Analytics_Username$] WITH PASSWORD = '$Analytics_Password$'
ALTER ROLE [db_datareader] ADD MEMBER [$Analytics_Username$]
GRANT SELECT ON SCHEMA :: [dbo] to [AnalyticsUser];