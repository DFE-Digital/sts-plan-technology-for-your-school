const Database = require('better-sqlite3');
const { generateUuid } = require('./utils');

function initializeSqliteDb(dbPath) {
  const db = new Database(dbPath);
  db.pragma('journal_mode = WAL');
  db.pragma('foreign_keys = ON');

  db.exec(`
    CREATE TABLE IF NOT EXISTS users (
      user_id TEXT PRIMARY KEY,
      email TEXT NOT NULL UNIQUE,
      given_name TEXT,
      family_name TEXT,
      user_status INTEGER,
      user_source TEXT NOT NULL DEFAULT 'DfE Sign In',
      created_at TEXT DEFAULT CURRENT_TIMESTAMP
    );
    CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
    CREATE INDEX IF NOT EXISTS idx_users_source ON users(user_source);
  `);

  db.exec(`
    CREATE TABLE IF NOT EXISTS organisations (
      organisation_id TEXT PRIMARY KEY,
      name TEXT NOT NULL,
      category_id TEXT,
      category_name TEXT,
      urn TEXT,
      uid TEXT,
      ukprn TEXT,
      upin TEXT,
      establishment_number TEXT,
      status INTEGER,
      status_name TEXT,
      closed_on TEXT,
      address TEXT,
      telephone TEXT,
      statutory_low_age INTEGER,
      statutory_high_age INTEGER,
      legacy_id TEXT,
      company_registration_number TEXT,
      provider_profile_id TEXT,
      provider_type_name TEXT,
      legal_name TEXT,
      source_system TEXT,
      gias_provider_type TEXT,
      pims_provider_type TEXT,
      pims_provider_type_code INTEGER,
      pims_status TEXT,
      pims_status_name TEXT,
      gias_status TEXT,
      gias_status_name TEXT,
      master_provider_status_code INTEGER,
      master_provider_status_name TEXT,
      opened_on TEXT,
      district_administrative_code TEXT,
      district_administrative_name TEXT,
      created_at TEXT DEFAULT CURRENT_TIMESTAMP
    );
    CREATE INDEX IF NOT EXISTS idx_orgs_name ON organisations(name);
    CREATE INDEX IF NOT EXISTS idx_orgs_ukprn ON organisations(ukprn);
    CREATE INDEX IF NOT EXISTS idx_orgs_upin ON organisations(upin);
  `);

  db.exec(`
    CREATE TABLE IF NOT EXISTS services (
      service_id TEXT PRIMARY KEY,
      organisation_id TEXT NOT NULL,
      service_name TEXT NOT NULL,
      service_description TEXT,
      created_at TEXT DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (organisation_id) REFERENCES organisations(organisation_id)
    );
    CREATE INDEX IF NOT EXISTS idx_services_org ON services(organisation_id);
  `);

  db.exec(`
    CREATE TABLE IF NOT EXISTS roles (
      role_id TEXT PRIMARY KEY,
      service_id TEXT NOT NULL,
      role_name TEXT NOT NULL,
      role_code TEXT,
      created_at TEXT DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (service_id) REFERENCES services(service_id)
    );
    CREATE INDEX IF NOT EXISTS idx_roles_service ON roles(service_id);
  `);

  db.exec(`
    CREATE TABLE IF NOT EXISTS user_organisation_mappings (
      mapping_id TEXT PRIMARY KEY,
      user_id TEXT NOT NULL,
      organisation_id TEXT NOT NULL,
      org_role_id INTEGER,
      org_role_name TEXT,
      created_at TEXT DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (user_id) REFERENCES users(user_id),
      FOREIGN KEY (organisation_id) REFERENCES organisations(organisation_id),
      UNIQUE(user_id, organisation_id)
    );
    CREATE INDEX IF NOT EXISTS idx_user_org_user ON user_organisation_mappings(user_id);
    CREATE INDEX IF NOT EXISTS idx_user_org_org ON user_organisation_mappings(organisation_id);
  `);

  db.exec(`
    CREATE TABLE IF NOT EXISTS user_service_roles (
      mapping_id TEXT PRIMARY KEY,
      user_id TEXT NOT NULL,
      service_id TEXT NOT NULL,
      role_id TEXT NOT NULL,
      created_at TEXT DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (user_id) REFERENCES users(user_id),
      FOREIGN KEY (service_id) REFERENCES services(service_id),
      FOREIGN KEY (role_id) REFERENCES roles(role_id),
      UNIQUE(user_id, service_id, role_id)
    );
    CREATE INDEX IF NOT EXISTS idx_user_service_user ON user_service_roles(user_id);
    CREATE INDEX IF NOT EXISTS idx_user_service_service ON user_service_roles(service_id);
  `);

  return db;
}

function upsertUser(
  db,
  userId,
  email,
  givenName,
  familyName,
  userStatus,
  userSource = 'DfE Sign In',
) {
  db.prepare(
    `
    INSERT OR IGNORE INTO users (user_id, email, given_name, family_name, user_status, user_source)
    VALUES (?, ?, ?, ?, ?, ?)
  `,
  ).run(userId, email, givenName, familyName, userStatus, userSource);
}

const ORG_COLUMNS = `
  organisation_id, name, category_id, category_name, urn, uid, ukprn, upin,
  establishment_number, status, status_name, closed_on, address, telephone,
  statutory_low_age, statutory_high_age, legacy_id, company_registration_number,
  provider_profile_id, provider_type_name, legal_name, source_system,
  gias_provider_type, pims_provider_type, pims_provider_type_code,
  pims_status, pims_status_name, gias_status, gias_status_name,
  master_provider_status_code, master_provider_status_name, opened_on,
  district_administrative_code, district_administrative_name
`;

const ORG_PLACEHOLDERS =
  '?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?';

function orgValues(org) {
  return [
    org.id,
    org.name,
    org.category?.id,
    org.category?.name,
    org.urn,
    org.uid,
    org.ukprn,
    org.upin,
    org.establishmentNumber,
    org.status?.id,
    org.status?.name,
    org.closedOn,
    org.address,
    org.telephone,
    org.statutoryLowAge,
    org.statutoryHighAge,
    org.legacyId,
    org.companyRegistrationNumber,
    org.ProviderProfileID,
    org.ProviderTypeName,
    org.LegalName,
    org.SourceSystem,
    org.GIASProviderType,
    org.PIMSProviderType,
    org.PIMSProviderTypeCode,
    org.PIMSStatus,
    org.PIMSStatusName,
    org.GIASStatus,
    org.GIASStatusName,
    org.MasterProviderStatusCode,
    org.MasterProviderStatusName,
    org.OpenedOn,
    org.DistrictAdministrativeCode,
    org.DistrictAdministrativeName,
  ];
}

// Full upsert — overwrites existing row. Use for users with complete v2/organisations data.
function upsertOrganisation(db, org) {
  db.prepare(
    `INSERT OR REPLACE INTO organisations (${ORG_COLUMNS}) VALUES (${ORG_PLACEHOLDERS})`,
  ).run(orgValues(org));
}

// Soft insert — skips if org already exists. Use for discovered users to avoid downgrading full data.
function insertOrgIfMissing(db, org) {
  db.prepare(
    `INSERT OR IGNORE INTO organisations (${ORG_COLUMNS}) VALUES (${ORG_PLACEHOLDERS})`,
  ).run(orgValues(org));
}

function insertService(db, serviceId, organisationId, serviceName, serviceDescription) {
  db.prepare(
    `
    INSERT OR IGNORE INTO services (service_id, organisation_id, service_name, service_description)
    VALUES (?, ?, ?, ?)
  `,
  ).run(serviceId, organisationId, serviceName, serviceDescription);
}

function upsertRole(db, roleId, serviceId, roleName, roleCode) {
  db.prepare(
    `
    INSERT OR IGNORE INTO roles (role_id, service_id, role_name, role_code)
    VALUES (?, ?, ?, ?)
  `,
  ).run(roleId, serviceId, roleName, roleCode);
}

function insertUserOrgMapping(db, userId, organisationId, orgRoleId, orgRoleName) {
  db.prepare(
    `
    INSERT OR IGNORE INTO user_organisation_mappings (mapping_id, user_id, organisation_id, org_role_id, org_role_name)
    VALUES (?, ?, ?, ?, ?)
  `,
  ).run(generateUuid(), userId, organisationId, orgRoleId, orgRoleName);
}

function insertUserServiceRole(db, userId, serviceId, roleId) {
  db.prepare(
    `
    INSERT OR IGNORE INTO user_service_roles (mapping_id, user_id, service_id, role_id)
    VALUES (?, ?, ?, ?)
  `,
  ).run(generateUuid(), userId, serviceId, roleId);
}

module.exports = {
  initializeSqliteDb,
  upsertUser,
  upsertOrganisation,
  insertOrgIfMissing,
  insertService,
  upsertRole,
  insertUserOrgMapping,
  insertUserServiceRole,
};
