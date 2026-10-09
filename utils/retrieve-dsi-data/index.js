require('dotenv').config();
const fs = require('fs');
const path = require('path');
const csv = require('csv-parser');

const {
  initializeSqliteDb,
  upsertUser,
  upsertOrganisation,
  insertOrgIfMissing,
  insertService,
  upsertRole,
  insertUserOrgMapping,
  insertUserServiceRole,
} = require('./lib/db');
const {
  fetchUserOrgServices,
  fetchUserOrgsV2,
  fetchServiceUsersPage,
  normalizeDiscoveredOrg,
} = require('./lib/api');
const { validateDatabase } = require('./lib/validate');
const { generateTSqlFromDb, generateCsvFromDb } = require('./lib/reports');
const { generateUuid } = require('./lib/utils');

const DISCOVERY_MODE = process.env.DISCOVERY_MODE === 'true';
const CSV_FILENAME_PREFIX = 'dsi-user-data';
const SQL_FILENAME_PREFIX = 'dsi-user-data';
const pwd = process.cwd();

async function processCsv() {
  console.log('🚀 Starting DSI data retrieval...');
  console.log(`   API Backoff: ${process.env.API_BACKOFF_MS || 100}ms`);
  console.log(`   Discovery Mode: ${DISCOVERY_MODE ? 'ENABLED' : 'DISABLED'}\n`);

  // Ensure output directories exist
  const dataDir = path.join(pwd, 'data');
  const outputsDir = path.join(pwd, 'outputs');
  if (!fs.existsSync(dataDir)) fs.mkdirSync(dataDir);
  if (!fs.existsSync(outputsDir)) fs.mkdirSync(outputsDir);
  console.log(`   Data directory:   ${dataDir}`);
  console.log(`   Output directory: ${outputsDir}\n`);

  const now = new Date();
  const dateStamp = now.toISOString().slice(0, 10).replace(/-/g, '');
  const dbPath = path.join(dataDir, `dsi-${dateStamp}.db`);
  const csvFileName = path.join(outputsDir, `${CSV_FILENAME_PREFIX}-${dateStamp}.csv`);
  const sqlFileName = path.join(outputsDir, `${SQL_FILENAME_PREFIX}-${dateStamp}.sql`);

  // Phase 1: Initialise database
  console.log('📦 Initialising SQLite database...');
  const db = initializeSqliteDb(dbPath);
  console.log(`   Created: ${dbPath}\n`);

  // Phase 2: Read input CSV
  console.log('📖 Reading input CSV...');
  const signInReferences = [];
  await new Promise((resolve, reject) => {
    fs.createReadStream(path.join(dataDir, 'inputs.csv'))
      .pipe(csv())
      .on('data', (row) => {
        // Use dfeSignInRef column if present, otherwise fall back to first column
        const reference = (row['dfeSignInRef'] ?? row[Object.keys(row)[0]])?.trim();
        if (reference) signInReferences.push(reference);
      })
      .on('end', () => {
        console.log(`   Found ${signInReferences.length} users\n`);
        resolve();
      })
      .on('error', reject);
  });

  // Phase 3: Process users
  console.log('👥 Processing users...');
  const userIds = new Set();
  let processedCount = 1;
  let userCount = 0;

  for (const userRef of signInReferences) {
    try {
      const [orgData, userData] = await Promise.all([
        fetchUserOrgsV2(userRef),
        fetchUserOrgServices(userRef),
      ]);

      // Upsert organisations first (required before user-org mappings due to FK)
      if (Array.isArray(orgData)) {
        orgData.forEach((org) => upsertOrganisation(db, org));
      }

      console.log(`   Processing user ${processedCount++} of ${signInReferences.length}:`);
      console.log(`     ${userData.email} (${userData.userId})`);
      upsertUser(
        db,
        userData.userId,
        userData.email,
        userData.givenName,
        userData.familyName,
        userData.userStatus,
        'DfE Sign In',
      );
      userIds.add(userData.userId);
      userCount++;

      if (Array.isArray(userData.organisations)) {
        userData.organisations.forEach((org) => {
          console.log(`     - Organisation: ${org.name} (${org.id})`);
          insertUserOrgMapping(db, userData.userId, org.id, org.orgRoleId, org.orgRoleName);

          if (Array.isArray(org.services)) {
            org.services.forEach((service) => {
              console.log(`       - Service: ${service.name}`);
              const serviceId = generateUuid();
              insertService(db, serviceId, org.id, service.name, service.description);

              if (Array.isArray(service.roles)) {
                service.roles.forEach((role) => {
                  console.log(`         - Role: ${role.name} (${role.code})`);
                  const roleId = generateUuid();
                  upsertRole(db, roleId, serviceId, role.name, role.code);
                  insertUserServiceRole(db, userData.userId, serviceId, roleId);
                });
              }
            });
          }
        });
      }

      console.log(`   ✓ ${userData.email}`);
    } catch (error) {
      console.error(`   ❌ Error processing user ${userRef}: ${error.message}`);
    }
  }

  console.log(`\n   Total users processed: ${userCount}\n`);

  // Phase 4: Discovery — paginate GET /users to find all users in this service's scope
  if (DISCOVERY_MODE) {
    console.log('🔍 Discovery Mode: Fetching all users from service scope...');

    let page = 1;
    let totalPages = 1;
    let discoveredCount = 0;
    const discoveredUserIds = new Set();

    do {
      let result;
      try {
        result = await fetchServiceUsersPage(page, 100);
      } catch (error) {
        console.error(`   ❌ Failed to fetch page ${page}: ${error.message}`);
        break;
      }

      totalPages = result.numberOfPages || 1;
      console.log(`   Page ${page}/${totalPages} (${result.users?.length ?? 0} records)`);

      for (const user of result.users || []) {
        if (!user.userId) continue;
        if (userIds.has(user.userId)) continue; // already fully processed in Phase 3

        const isNew = !discoveredUserIds.has(user.userId);
        if (isNew) {
          discoveredUserIds.add(user.userId);
          upsertUser(
            db,
            user.userId,
            user.email,
            user.givenName,
            user.familyName,
            user.userStatus,
            'Discovered',
          );
          discoveredCount++;
        }

        // Each record represents one user-org pair; process the org mapping even for repeat appearances
        if (user.organisation?.id) {
          insertOrgIfMissing(db, normalizeDiscoveredOrg(user.organisation));
          insertUserOrgMapping(db, user.userId, user.organisation.id, user.roleId, user.roleName);
        }
      }

      page++;
    } while (page <= totalPages);

    console.log(`   Discovered ${discoveredCount} additional users\n`);
  }

  // Phase 5: Validate database
  const validationErrors = validateDatabase(db);

  if (validationErrors.length > 0) {
    console.error('\n❌ VALIDATION FAILED\n');
    validationErrors.forEach((error) => console.error(`  ${error}`));
    console.error('\nOutputs NOT generated due to validation errors.');
    db.close();
    process.exit(1);
  }

  // Phase 6: Generate outputs
  console.log('\n📝 Generating SQL script...');
  fs.writeFileSync(sqlFileName, generateTSqlFromDb(db, DISCOVERY_MODE));
  console.log(`   ✓ Created: ${sqlFileName}`);

  console.log('📊 Generating CSV report...');
  fs.writeFileSync(csvFileName, generateCsvFromDb(db));
  console.log(`   ✓ Created: ${csvFileName}`);

  db.close();

  console.log(`\n✅ Complete!`);
  console.log(`\n   data/    ${dbPath}`);
  console.log(`   outputs/ ${sqlFileName}`);
  console.log(`   outputs/ ${csvFileName}\n`);
}

processCsv().catch((error) => {
  console.error('Fatal error:', error);
  process.exit(1);
});
