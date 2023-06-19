-- <summary>
-- Create STS Users table.
-- </summary>
CREATE TABLE STSUsers (
    URN varchar(255),
    Username varchar(255)
);

-- <summary>
-- Create table to track user activity.
-- </summary>
CREATE TABLE STSTrackUsers (
    URN varchar(255),
    Username varchar(255)
);

-- <summary>
-- Create table to track user answers.
-- </summary>
CREATE TABLE STSTrackAnsers (
    URN varchar(255),
    Username varchar(255),
    QuestionNumber int,
    QuestionAnswer int
);

-- <summary>
-- Create CacheTable from parameters passed in.
-- </summary>
--CREATE TABLE $CacheTable (
--            Id nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
--            Value varbinary(MAX) NOT NULL,
 --           ExpiresAtTime datetimeoffset NOT NULL,
--            SlidingExpirationInSeconds bigint NULL,
--            AbsoluteExpiration datetimeoffset NULL,
--            PRIMARY KEY (Id));
--
--CREATE NONCLUSTERED INDEX Index_ExpiresAtTime ON $CacheSchema.$CacheTable(ExpiresAtTime);