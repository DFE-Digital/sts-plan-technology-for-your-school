const { Parser } = require('json2csv');
const { generateUuid } = require('./utils');

function escapeSql(value) {
  if (value === null || value === undefined) return 'NULL';
  return `'${String(value).replace(/'/g, "''")}'`;
}

function generateSqlSchema() {
  return `
-- Create dsi schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dsi')
BEGIN
  EXEC('CREATE SCHEMA dsi');
END;
GO

-- Drop existing tables if they exist (in reverse dependency order)
IF OBJECT_ID('dsi.UserServiceRoles', 'U') IS NOT NULL DROP TABLE dsi.UserServiceRoles;
IF OBJECT_ID('dsi.Roles', 'U') IS NOT NULL DROP TABLE dsi.Roles;
IF OBJECT_ID('dsi.Services', 'U') IS NOT NULL DROP TABLE dsi.Services;
IF OBJECT_ID('dsi.UserOrganisationMappings', 'U') IS NOT NULL DROP TABLE dsi.UserOrganisationMappings;
IF OBJECT_ID('dsi.Organisations', 'U') IS NOT NULL DROP TABLE dsi.Organisations;
IF OBJECT_ID('dsi.Users', 'U') IS NOT NULL DROP TABLE dsi.Users;
GO

-- Create Users table
CREATE TABLE dsi.Users (
  UserId UNIQUEIDENTIFIER PRIMARY KEY,
  Email NVARCHAR(255) NOT NULL,
  GivenName NVARCHAR(100),
  FamilyName NVARCHAR(100),
  UserStatus INT,
  UserSource NVARCHAR(50) DEFAULT 'DfE Sign In',
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);
CREATE INDEX IX_Users_Email ON dsi.Users(Email);
CREATE INDEX IX_Users_Source ON dsi.Users(UserSource);
GO

-- Create Organisations table
CREATE TABLE dsi.Organisations (
  OrganisationId UNIQUEIDENTIFIER PRIMARY KEY,
  Name NVARCHAR(255) NOT NULL,
  CategoryId NVARCHAR(10),
  CategoryName NVARCHAR(100),
  URN NVARCHAR(50),
  UID NVARCHAR(50),
  UKPRN NVARCHAR(50),
  UPIN NVARCHAR(50),
  EstablishmentNumber NVARCHAR(50),
  Status INT,
  StatusName NVARCHAR(50),
  ClosedOn DATETIME,
  Address NVARCHAR(MAX),
  Telephone NVARCHAR(20),
  StatutoryLowAge INT,
  StatutoryHighAge INT,
  LegacyId NVARCHAR(100),
  CompanyRegistrationNumber NVARCHAR(50),
  ProviderProfileID NVARCHAR(100),
  ProviderTypeName NVARCHAR(255),
  LegalName NVARCHAR(255),
  SourceSystem NVARCHAR(50),
  GIASProviderType NVARCHAR(100),
  PIMSProviderType NVARCHAR(100),
  PIMSProviderTypeCode INT,
  PIMSStatus NVARCHAR(50),
  PIMSStatusName NVARCHAR(100),
  GIASStatus NVARCHAR(50),
  GIASStatusName NVARCHAR(100),
  MasterProviderStatusCode INT,
  MasterProviderStatusName NVARCHAR(100),
  OpenedOn DATETIME,
  DistrictAdministrativeCode NVARCHAR(50),
  DistrictAdministrativeName NVARCHAR(255),
  CreatedAt DATETIME DEFAULT GETUTCDATE()
);
CREATE INDEX IX_Organisations_Name ON dsi.Organisations(Name);
CREATE INDEX IX_Organisations_UKPRN ON dsi.Organisations(UKPRN);
CREATE INDEX IX_Organisations_UPIN ON dsi.Organisations(UPIN);
GO

-- Create Services table
CREATE TABLE dsi.Services (
  ServiceId UNIQUEIDENTIFIER PRIMARY KEY,
  OrganisationId UNIQUEIDENTIFIER NOT NULL,
  ServiceName NVARCHAR(255) NOT NULL,
  ServiceDescription NVARCHAR(MAX),
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  FOREIGN KEY (OrganisationId) REFERENCES dsi.Organisations(OrganisationId)
);
CREATE INDEX IX_Services_OrgId ON dsi.Services(OrganisationId);
GO

-- Create Roles table
CREATE TABLE dsi.Roles (
  RoleId UNIQUEIDENTIFIER PRIMARY KEY,
  ServiceId UNIQUEIDENTIFIER NOT NULL,
  RoleName NVARCHAR(255) NOT NULL,
  RoleCode NVARCHAR(100),
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  FOREIGN KEY (ServiceId) REFERENCES dsi.Services(ServiceId)
);
CREATE INDEX IX_Roles_ServiceId ON dsi.Roles(ServiceId);
GO

-- Create UserOrganisationMappings table
CREATE TABLE dsi.UserOrganisationMappings (
  UserOrganisationMappingId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  UserId UNIQUEIDENTIFIER NOT NULL,
  OrganisationId UNIQUEIDENTIFIER NOT NULL,
  OrgRoleId INT,
  OrgRoleName NVARCHAR(255),
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  FOREIGN KEY (UserId) REFERENCES dsi.Users(UserId),
  FOREIGN KEY (OrganisationId) REFERENCES dsi.Organisations(OrganisationId)
);
CREATE INDEX IX_UserOrg_UserId ON dsi.UserOrganisationMappings(UserId);
CREATE INDEX IX_UserOrg_OrgId ON dsi.UserOrganisationMappings(OrganisationId);
GO

-- Create UserServiceRoles table
CREATE TABLE dsi.UserServiceRoles (
  UserServiceRoleId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
  UserId UNIQUEIDENTIFIER NOT NULL,
  ServiceId UNIQUEIDENTIFIER NOT NULL,
  RoleId UNIQUEIDENTIFIER NOT NULL,
  CreatedAt DATETIME DEFAULT GETUTCDATE(),
  FOREIGN KEY (UserId) REFERENCES dsi.Users(UserId),
  FOREIGN KEY (ServiceId) REFERENCES dsi.Services(ServiceId),
  FOREIGN KEY (RoleId) REFERENCES dsi.Roles(RoleId)
);
CREATE INDEX IX_UserServiceRole_UserId ON dsi.UserServiceRoles(UserId);
CREATE INDEX IX_UserServiceRole_ServiceId ON dsi.UserServiceRoles(ServiceId);
CREATE INDEX IX_UserServiceRole_RoleId ON dsi.UserServiceRoles(RoleId);
GO
`;
}

function generateTSqlFromDb(db, discoveryMode = false) {
  let sql = `
-- DSI Data Import Script
-- Generated: ${new Date().toISOString()}
-- Mode: ${discoveryMode ? 'DISCOVERY' : 'SEED_ONLY'}

USE [YourDatabaseName];
GO

`;

  sql += generateSqlSchema();
  sql += `\n-- Insert data\n`;

  const userIdToUuid = {};
  const orgIdToUuid = {};
  const serviceIdToUuid = {};
  const roleIdToUuid = {};

  db.prepare('SELECT * FROM users')
    .all()
    .forEach((user) => {
      const uuid = generateUuid();
      userIdToUuid[user.user_id] = uuid;
      sql += `INSERT INTO dsi.Users (UserId, Email, GivenName, FamilyName, UserStatus, UserSource) VALUES (${[
        escapeSql(uuid),
        escapeSql(user.email),
        escapeSql(user.given_name),
        escapeSql(user.family_name),
        user.user_status !== null ? user.user_status : 'NULL',
        escapeSql(user.user_source),
      ].join(', ')});\n`;
    });

  db.prepare('SELECT * FROM organisations')
    .all()
    .forEach((org) => {
      const uuid = generateUuid();
      orgIdToUuid[org.organisation_id] = uuid;
      sql += `INSERT INTO dsi.Organisations (OrganisationId, Name, CategoryId, CategoryName, URN, UID, UKPRN, UPIN, EstablishmentNumber, Status, StatusName, ClosedOn, Address, Telephone, StatutoryLowAge, StatutoryHighAge, LegacyId, CompanyRegistrationNumber, ProviderProfileID, ProviderTypeName, LegalName, SourceSystem, GIASProviderType, PIMSProviderType, PIMSProviderTypeCode, PIMSStatus, PIMSStatusName, GIASStatus, GIASStatusName, MasterProviderStatusCode, MasterProviderStatusName, OpenedOn, DistrictAdministrativeCode, DistrictAdministrativeName) VALUES (${[
        escapeSql(uuid),
        escapeSql(org.name),
        escapeSql(org.category_id),
        escapeSql(org.category_name),
        escapeSql(org.urn),
        escapeSql(org.uid),
        escapeSql(org.ukprn),
        escapeSql(org.upin),
        escapeSql(org.establishment_number),
        org.status !== null ? org.status : 'NULL',
        escapeSql(org.status_name),
        org.closed_on ? escapeSql(org.closed_on) : 'NULL',
        escapeSql(org.address),
        escapeSql(org.telephone),
        org.statutory_low_age !== null ? org.statutory_low_age : 'NULL',
        org.statutory_high_age !== null ? org.statutory_high_age : 'NULL',
        escapeSql(org.legacy_id),
        escapeSql(org.company_registration_number),
        escapeSql(org.provider_profile_id),
        escapeSql(org.provider_type_name),
        escapeSql(org.legal_name),
        escapeSql(org.source_system),
        escapeSql(org.gias_provider_type),
        escapeSql(org.pims_provider_type),
        org.pims_provider_type_code !== null ? org.pims_provider_type_code : 'NULL',
        escapeSql(org.pims_status),
        escapeSql(org.pims_status_name),
        escapeSql(org.gias_status),
        escapeSql(org.gias_status_name),
        org.master_provider_status_code !== null ? org.master_provider_status_code : 'NULL',
        escapeSql(org.master_provider_status_name),
        org.opened_on ? escapeSql(org.opened_on) : 'NULL',
        escapeSql(org.district_administrative_code),
        escapeSql(org.district_administrative_name),
      ].join(', ')});\n`;
    });

  db.prepare('SELECT * FROM services')
    .all()
    .forEach((service) => {
      const uuid = generateUuid();
      serviceIdToUuid[service.service_id] = uuid;
      sql += `INSERT INTO dsi.Services (ServiceId, OrganisationId, ServiceName, ServiceDescription) VALUES (${[
        escapeSql(uuid),
        escapeSql(orgIdToUuid[service.organisation_id]),
        escapeSql(service.service_name),
        escapeSql(service.service_description),
      ].join(', ')});\n`;
    });

  db.prepare('SELECT * FROM roles')
    .all()
    .forEach((role) => {
      const uuid = generateUuid();
      roleIdToUuid[role.role_id] = uuid;
      sql += `INSERT INTO dsi.Roles (RoleId, ServiceId, RoleName, RoleCode) VALUES (${[
        escapeSql(uuid),
        escapeSql(serviceIdToUuid[role.service_id]),
        escapeSql(role.role_name),
        escapeSql(role.role_code),
      ].join(', ')});\n`;
    });

  db.prepare('SELECT * FROM user_organisation_mappings')
    .all()
    .forEach((mapping) => {
      sql += `INSERT INTO dsi.UserOrganisationMappings (UserOrganisationMappingId, UserId, OrganisationId, OrgRoleId, OrgRoleName) VALUES (${[
        escapeSql(generateUuid()),
        escapeSql(userIdToUuid[mapping.user_id]),
        escapeSql(orgIdToUuid[mapping.organisation_id]),
        mapping.org_role_id !== null ? mapping.org_role_id : 'NULL',
        escapeSql(mapping.org_role_name),
      ].join(', ')});\n`;
    });

  db.prepare('SELECT * FROM user_service_roles')
    .all()
    .forEach((mapping) => {
      sql += `INSERT INTO dsi.UserServiceRoles (UserServiceRoleId, UserId, ServiceId, RoleId) VALUES (${[
        escapeSql(generateUuid()),
        escapeSql(userIdToUuid[mapping.user_id]),
        escapeSql(serviceIdToUuid[mapping.service_id]),
        escapeSql(roleIdToUuid[mapping.role_id]),
      ].join(', ')});\n`;
    });

  sql += `
GO

-- Summary statistics
SELECT 'Users' as EntityType, COUNT(*) as Count FROM dsi.Users
UNION ALL SELECT 'Organisations', COUNT(*) FROM dsi.Organisations
UNION ALL SELECT 'Services', COUNT(*) FROM dsi.Services
UNION ALL SELECT 'Roles', COUNT(*) FROM dsi.Roles
UNION ALL SELECT 'User-Org Mappings', COUNT(*) FROM dsi.UserOrganisationMappings
UNION ALL SELECT 'User-Service Roles', COUNT(*) FROM dsi.UserServiceRoles;
GO
`;

  return sql;
}

function generateCsvFromDb(db) {
  const rows = db
    .prepare(
      `
    SELECT
      u.email,
      u.given_name || ' ' || u.family_name as name,
      u.user_source,
      COUNT(DISTINCT uom.organisation_id) as organisation_count
    FROM users u
    LEFT JOIN user_organisation_mappings uom ON u.user_id = uom.user_id
    GROUP BY u.user_id, u.email, u.given_name, u.family_name, u.user_source
    ORDER BY u.email
  `,
    )
    .all();

  return new Parser({ fields: ['email', 'name', 'user_source', 'organisation_count'] }).parse(rows);
}

module.exports = { generateTSqlFromDb, generateCsvFromDb };
