# Code Enhancement Reference

## Summary of Architecture

The application has been refactored into a **modular architecture** designed for scalability and
maintainability:

- **`index.js`** (152 lines) - Main orchestration with 6-phase processing pipeline
- **`lib/db.js`** - Database layer with SQLite CRUD operations
- **`lib/api.js`** - DfE Sign-In API client with JWT caching
- **`lib/validate.js`** - Database integrity validation
- **`lib/reports.js`** - T-SQL and CSV report generation
- **`lib/utils.js`** - Utility functions (UUID generation, sleep)

This modular design supports:

- CSV and T-SQL dual output
- Comprehensive data collection (users, orgs, services, roles)
- Discovery mode for finding additional users
- SQLite validation layer with FK constraints

---

## Project Structure

```
utils/retrieve-dsi-data/
├── index.js                           # Main entry point (orchestration)
├── lib/
│   ├── api.js                        # API client & JWT handling
│   ├── db.js                         # Database layer & schema
│   ├── reports.js                    # SQL/CSV generation
│   ├── utils.js                      # Utilities (UUID, sleep)
│   └── validate.js                   # Integrity validation
├── data/                              # Inputs & SQLite databases
├── outputs/                           # Generated SQL & CSV reports
├── package.json
├── .env.example
├── README.md
├── CODE_ENHANCEMENT_REFERENCE.md     # This file
├── DSI_SQL_IMPLEMENTATION_GUIDE.md    # SQL schema documentation
└── .gitignore
```

---

## Core Modules

### `lib/api.js` - API Client Layer

**Purpose:** Handles all DfE Sign-In API communication with JWT authentication and token caching.

**Notes:** Uses the [DfE Login Public API](https://github.com/DFE-Digital/login.dfe.public-api)

**Key Functions:**

| Function                                | Endpoint                               | Purpose                                     |
| --------------------------------------- | -------------------------------------- | ------------------------------------------- |
| `generateJwt()`                         | N/A                                    | Creates HS256 JWT token; caches for 4.5 min |
| `fetchUserOrgServices(userRef)`         | GET `/users/{id}/organisationservices` | Fetches user's org/service/role data        |
| `fetchUserOrgsV2(userRef)`              | GET `/users/{id}/v2/organisations`     | Fetches enhanced org data (34 fields)       |
| `fetchServiceUsersPage(page, pageSize)` | GET `/users?page={p}&pageSize={ps}`    | Discovery mode: paginated user list         |
| `normalizeDiscoveredOrg(rawOrg)`        | N/A                                    | Maps discovered org format to schema        |

**Features:**

- **Token Caching**: JWT tokens valid 5 minutes but cached for 4.5 to avoid redundant generation
- **API Backoff**: Configurable delay between requests (default 100ms via `API_BACKOFF_MS`)
- **Error Handling**: Logs API failures but continues processing

**Configuration:**

```javascript
const API_BACKOFF_MS = parseInt(process.env.API_BACKOFF_MS || '100', 10);
```

---

### `lib/db.js` - Database Layer

**Purpose:** All database operations using better-sqlite3 with WAL mode and FK constraints.

**Schema Initialization:**

```javascript
initializeSqliteDb(dbPath);
```

Creates 6-table schema:

| Table                        | Purpose                               |
| ---------------------------- | ------------------------------------- |
| `users`                      | User information with source tracking |
| `organisations`              | Comprehensive org data (34 fields)    |
| `services`                   | Services per organization             |
| `roles`                      | Roles within services                 |
| `user_organisation_mappings` | User-org relationships                |
| `user_service_roles`         | User roles in services                |

**CRUD Operations:**

```javascript
upsertUser(db, userId, email, givenName, familyName, userStatus, userSource);
upsertOrganisation(db, org); // Full upsert (overwrites)
insertOrgIfMissing(db, org); // Soft insert (skips if exists)
insertService(db, serviceId, organisationId, serviceName, description);
upsertRole(db, roleId, serviceId, roleName, roleCode);
insertUserOrgMapping(db, userId, organisationId, orgRoleId, orgRoleName);
insertUserServiceRole(db, userId, serviceId, roleId);
```

**User Source Tracking:**

The `user_source` field distinguishes user origins:

- `'DfE Sign In'` - Users from seed CSV (Phase 3)
- `'Discovered'` - Additional users found via paginated endpoint (Phase 4)

**Constraints:**

- Foreign keys enabled: `PRAGMA foreign_keys = ON`
- Write-ahead logging: `PRAGMA journal_mode = WAL` (faster, concurrent access)
- Unique constraints prevent duplicate mappings
- Indexes on frequently queried columns (email, UKPRN, etc.)

---

### `lib/validate.js` - Validation Layer

**Purpose:** Ensures data integrity before SQL generation.

```javascript
validateDatabase(db) → Array<string>
```

**Checks performed:**

1. **Orphaned foreign keys**: User-org mappings with missing user or org
2. **Orphaned roles**: Roles referencing non-existent services
3. **Orphaned user-service-roles**: Any mapping with missing references
4. **Duplicate emails**: Multiple users with same email

**Behavior:**

- Returns array of error messages (empty = valid)
- Prints validation summary with counts:
  - Total users, organizations, services, roles
  - User source breakdown (seed vs. discovered)
  - All mapping counts
- If errors exist, `processCsv()` calls `process.exit(1)` to halt execution

**Sample Output:**

```
📋 Validating database...
✅ Validation passed!
   Users: 150 (125 DSI references, 25 discovered)
   Organizations: 42
   Services: 18
   Roles: 47
   User-Org Mappings: 183
   User-Service-Role Mappings: 256
```

---

### `lib/reports.js` - Report Generation

**Purpose:** Converts SQLite data to T-SQL script and CSV report.

**Functions:**

1. **`escapeSql(value)`** - Escapes data for SQL safety
   - Single quotes doubled: `'` → `''`
   - NULL handling: `null` → `NULL`
   - Used in all INSERT statements

2. **`generateSqlSchema()`** - Creates complete T-SQL schema:
   - Schema creation (IF NOT EXISTS)
   - Safe DROP statements (reverse dependency order)
   - 6 CREATE TABLE statements with FK and indexes
   - Output: ~150 lines of T-SQL

3. **`generateTSqlFromDb(db, discoveryMode)`** - Full SQL generation:
   - Generates schema first
   - Builds ID → UUID maps for all entities
   - Generates INSERT statements for all records
   - Escapes all values for SQL injection prevention
   - Appends summary statistics query
   - Marks discovery mode in header comment
   - Output: Complete, executable T-SQL script

4. **`generateCsvFromDb(db)`** - CSV report generation:
   - Query: Users with email, name, source, org count
   - Uses json2csv library
   - Output: Simple CSV suitable for spreadsheet import

---

### `lib/utils.js` - Utilities

```javascript
generateUuid(); // Deterministic UUID v4 for SQLite→SQL Server mapping
sleepMs(ms); // Async sleep for API rate limiting
```

---

## Processing Pipeline

The application executes these 6 phases sequentially:

### Phase 1: Database Initialization

```javascript
const db = initializeSqliteDb(dbPath);
```

**Actions:**

- Create SQLite database: `data/dsi-YYYYMMDD.db`
- Enable WAL mode for performance
- Enforce foreign key constraints
- Create all 6 tables with indexes
- Print database path

---

### Phase 2: Read Input CSV

```javascript
fs.createReadStream(path.join(dataDir, 'inputs.csv'))
  .pipe(csv())
  .on('data', (row) => {
    const reference = row['dfeSignInRef'] ?? row[Object.keys(row)[0]];
    signInReferences.push(reference);
  });
```

**Actions:**

- Read `data/inputs.csv`
- Extract `dfeSignInRef` column (fallback to first column)
- Build array of user IDs to process
- Print count of users found

---

### Phase 3: Process Seed Users

**For each user (sequential):**

```javascript
const [orgData, userData] = await Promise.all([
  fetchUserOrgsV2(userRef), // /users/{id}/v2/organisations
  fetchUserOrgServices(userRef), // /users/{id}/organisationservices
]);
```

**Actions:**

1. Fetch both endpoints in parallel
2. Upsert organizations from both endpoints
3. Insert user (source = 'DfE Sign In')
4. Add to processed user set
5. For each organization:
   - Add user-org mapping
   - For each service: create service record
   - For each role: create role record, link user-service-role
6. Print detailed activity (user, org, service, role)

**Output:**

```
👥 Processing users...
   Processing user: john.doe@example.com (user-id-123)
     - Organisation: Example Council (org-id-1)
       - Service: Access Control
         - Role: Approver (APPROVER)
   ✓ john.doe@example.com
```

---

### Phase 4: Discovery Mode (optional, if `DISCOVERY_MODE=true`)

**Endpoint:** Paginated `GET /users?page={page}&pageSize={pageSize}`

**For each page (page 1 to N):**

```javascript
const result = await fetchServiceUsersPage(page, 100);
```

**For each user in page (not already seed user):**

1. If new user ID: insert as 'Discovered' source
2. Normalize org data (field mapping via `normalizeDiscoveredOrg()`)
3. Call `insertOrgIfMissing()` - soft insert (prevents downgrading full data)
4. Add user-org mapping (even if user not new)

**Behavior:**

- Stops when page count exhausted or API error
- Multiple mappings per user handled correctly (UNIQUE constraint per pair)
- Does NOT call per-user endpoints for discovered users (saves API calls)

**Output:**

```
🔍 Discovery Mode: Fetching all users from service scope...
   Scanning 42 organizations for 18 roles...
   Page 1/5 (100 records)
   Page 2/5 (100 records)
   Page 3/5 (100 records)
   Page 4/5 (100 records)
   Page 5/5 (67 records)
   Discovered 34 additional users
```

---

### Phase 5: Validation

```javascript
const validationErrors = validateDatabase(db);
if (validationErrors.length > 0) {
  console.error('\n❌ VALIDATION FAILED\n');
  validationErrors.forEach((err) => console.error(`  ${err}`));
  process.exit(1);
}
```

**Behavior:**

- Runs all FK and consistency checks
- Halts execution if any errors found
- Prints statistics if valid
- Prevents bad data from reaching SQL Server

---

### Phase 6: Generate Outputs

**Actions:**

1. Generate T-SQL script:

   ```javascript
   fs.writeFileSync(sqlFileName, generateTSqlFromDb(db, DISCOVERY_MODE));
   ```

2. Generate CSV report:

   ```javascript
   fs.writeFileSync(csvFileName, generateCsvFromDb(db));
   ```

3. Close database: `db.close()`

4. Print summary:

   ```
   ✅ Complete!

      data/    data/dsi-20250609.db
      outputs/ outputs/dsi-user-data-20250609.sql
      outputs/ outputs/dsi-user-data-20250609.csv
   ```

---

## Data Flow

```
┌─ Environment Setup
│  ├─ Read: CLIENT_ID, API_SECRET, API_URL
│  ├─ Validate: DISCOVERY_MODE, API_BACKOFF_MS
│  └─ Create: data/, outputs/ directories
│
├─→ Phase 1: Initialize Database
│  └─ Output: data/dsi-YYYYMMDD.db (empty, schema only)
│
├─→ Phase 2: Parse Input CSV
│  └─ Input: data/inputs.csv
│
├─→ Phase 3: Process Seed Users (sequential)
│  ├─ For each user: 2 API calls (parallel)
│  ├─ Upsert: users, organisations, services, roles
│  └─ Insert: mappings
│
├─→ Phase 4: [Optional] Discovery Mode (paginated)
│  ├─ For each page of /users endpoint
│  ├─ Insert: new users (source='Discovered')
│  ├─ Soft insert: organisations
│  └─ Add: user-org mappings
│
├─→ Phase 5: Validate Database
│  └─ Exit if errors found
│
└─→ Phase 6: Generate Outputs
   ├─ Output: outputs/dsi-user-data-YYYYMMDD.sql
   └─ Output: outputs/dsi-user-data-YYYYMMDD.csv
```

---

## Discovery Mode Explained

Discovery mode finds **all users with service access** beyond your seed list.

### Typical Use Cases

**Enable Discovery (`DISCOVERY_MODE=true`):**

- Building a comprehensive access control reference database
- Auditing all users with service access
- Identifying inactive or unexpected user-org relationships
- Complete organizational access mapping

**Disable Discovery (`DISCOVERY_MODE=false`, default):**

- Only extracting data for specific users in seed CSV
- Minimizing API calls for large organizations
- Targeted data exports for specific user groups

### How the Paginated Endpoint Works

The `/users` endpoint returns paginated results with each record as a **user-org pair**:

```
Page 1:
┌─────────────────────────────────────────────┐
│ User A (john@example.com)                  │
│   → Org 1: Approver                        │
├─────────────────────────────────────────────┤
│ User B (jane@example.com)                  │
│   → Org 1: Analyst                         │
├─────────────────────────────────────────────┤
│ User A (john@example.com) [duplicate!]     │
│   → Org 2: Viewer                          │
├─────────────────────────────────────────────┤
│ User C (bob@example.com)                   │
│   → Org 2: Approver                        │
└─────────────────────────────────────────────┘
```

### Duplicate Handling

**Problem:** Same user appears on multiple pages (one entry per org)

**Solution:**

- `discoveredUserIds` Set tracks new users
- Insert user only first time
- Add mapping for every appearance
- UNIQUE constraint on (user_id, organisation_id) prevents duplicate mappings

**Result:** User inserted once, but all their org relationships captured

---

## Configuration

### Required Environment Variables

```bash
CLIENT_ID=your-service-client-id          # From DSI Manage console
API_SECRET=your-api-secret                # From DSI Manage console
API_URL=https://api.signin.education.gov.uk/  # Or your environment's endpoint
```

### Optional Environment Variables

```bash
DISCOVERY_MODE=true|false                 # Default: false
API_BACKOFF_MS=100                        # Default: 100ms between API calls
```

### File Structure

```
data/
  inputs.csv                              # Required: list of user IDs
  dsi-YYYYMMDD.db                         # Created: SQLite database

outputs/
  dsi-user-data-YYYYMMDD.csv              # Created: Summary report
  dsi-user-data-YYYYMMDD.sql              # Created: T-SQL script
```

---

## SQL Generation Strategy

### ID Mapping (SQLite → SQL Server)

**Challenge:** SQLite uses TEXT IDs from API; SQL Server requires UNIQUEIDENTIFIER

**Solution:**

1. **During generation**, build UUID maps:

   ```javascript
   const userIdToUuid = {};
   const orgIdToUuid = {};
   const serviceIdToUuid = {};
   const roleIdToUuid = {};

   // Map each entity's ID to a UUID
   db.prepare('SELECT * FROM users')
     .all()
     .forEach((user) => {
       userIdToUuid[user.user_id] = generateUuid();
     });
   ```

2. **When generating INSERT statements**, reference mapped UUIDs:

   ```sql
   INSERT INTO dsi.Users (UserId, Email, GivenName, FamilyName)
   VALUES ('550e8400-e29b-41d4-a716-446655440000', 'user@example.com', 'John', 'Doe');
   ```

3. **Foreign keys** use the same mapped UUIDs for referential integrity

### SQL Injection Prevention

All data passed through `escapeSql()`:

```javascript
function escapeSql(value) {
  if (value === null || value === undefined) return 'NULL';
  return `'${String(value).replace(/'/g, "''")}'`;
}
```

**Safety guarantees:**

- Single quotes doubled: `O'Brien` → `'O''Brien'`
- NULL values unquoted: `NULL` (not `'NULL'`)
- All string data wrapped in quotes
- Prevents injection from API responses

### Generated Script Features

**Idempotent execution:** Safe to run multiple times

```sql
IF OBJECT_ID('dsi.Users', 'U') IS NOT NULL DROP TABLE dsi.Users;
GO
CREATE TABLE dsi.Users (...)
GO
```

**Schema creation:** Creates `dsi` schema if missing

```sql
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dsi')
BEGIN
  EXEC('CREATE SCHEMA dsi');
END;
```

**Summary statistics:** Final query shows imported counts

```sql
SELECT 'Users' as EntityType, COUNT(*) as Count FROM dsi.Users
UNION ALL SELECT 'Organisations', COUNT(*) FROM dsi.Organisations
...
```

---

## Error Handling

### API Errors

- **Caught and logged** to console with user reference
- **Processing continues** for other users
- **Partial data acceptable**: Script continues even if individual users fail
- **Error visibility**: Console shows which users/endpoints failed

### Validation Errors

- **Prevents SQL generation** if data integrity compromised
- **Detailed error messages** explain the problem
- **Process exits** with code 1 to signal failure to scripts/CI

### SQL Generation Errors

- **Gracefully handles** missing/malformed data via `escapeSql()`
- **NULL values** properly represented in SQL
- **String escaping** prevents SQL syntax errors

---

## Performance Notes

### Time Complexity

| Phase                | Complexity       | Notes                                |
| -------------------- | ---------------- | ------------------------------------ |
| Phase 2 (CSV parse)  | O(n)             | n = input users                      |
| Phase 3 (API calls)  | O(n × 2)         | 2 API calls per user, sequential     |
| Phase 4 (Discovery)  | O(p × 100)       | p = pages, 100 users/page            |
| Phase 5 (Validation) | O(t)             | t = total records                    |
| Phase 6 (Generation) | O(n + o + s + r) | n=users, o=orgs, s=services, r=roles |

### Space Complexity

- **SQLite database:** ~5-10 MB for 1000 users, 200 orgs
- **In-memory maps:** Minimal (ID mapping only)
- **Generated files:** ~2-5 MB for SQL script

### API Call Optimization

- **Parallel per-user**: Both endpoints called simultaneously via `Promise.all()`
- **API backoff**: Configurable delay to respect rate limits
- **Token caching**: Reduces JWT generation overhead
- **Discovery mode**: Uses paginated endpoint (fewer calls than per-user model)

---

## Testing

### Minimal Test Run

```bash
# Create test input
echo "dfeSignInRef
user-id-123" > data/inputs.csv

# Run with valid credentials
API_BACKOFF_MS=50 node index.js

# Check outputs
ls -la outputs/
```

### Verify Outputs

```bash
# Check SQL script
grep "CREATE TABLE" outputs/dsi-user-data-*.sql
# Should show 6 CREATE TABLE statements

# Count inserted records
grep "^INSERT INTO dsi.Users" outputs/dsi-user-data-*.sql | wc -l

# Check CSV
head -3 outputs/dsi-user-data-*.csv
```

### Enable Debug Logging

```bash
# Run in development
node -e "console.log(process.env.NODE_ENV)" index.js

# Enable discovery mode
DISCOVERY_MODE=true node index.js
```

---

## Extension Points

### Adding New Fields to Organizations

1. **Schema**: Add column to `CREATE TABLE dsi.Organisations` in `lib/reports.js`
2. **API**: Map from `/v2/organisations` response in `lib/db.js` `orgValues()` function
3. **Database**: Update column list in `ORG_COLUMNS` constant

### Adding New Data Sources

1. **API function**: Create new fetch function in `lib/api.js`
2. **Database**: Add new table or extend existing in `lib/db.js`
3. **Phase**: Insert into appropriate processing phase in `index.js`
4. **Reports**: Extend SQL generation in `lib/reports.js`

### Changing Output Format

1. **New format**: Create new generator in `lib/reports.js`
2. **Integration**: Call from Phase 6 in `index.js`
3. **File naming**: Update filename pattern with new extension

---

## Troubleshooting

### "UNIQUE constraint failed"

**Cause**: Duplicate insertion attempts in mappings

**Fix**: Check for duplicate rows in input CSV or ensure Phase 4 user deduplication logic runs

### "Foreign key constraint violated"

**Cause**: Validation caught orphaned records

**Fix**: Review Phase 3 and Phase 4 output; ensure all organizations/services/roles inserted before
mappings

### "API authentication failed"

**Cause**: Invalid CLIENT_ID or API_SECRET

**Fix**: Verify credentials from DSI Manage console; check `.env` file

### "No outputs generated"

**Cause**: Validation errors or no data processed

**Fix**: Check console output for phase errors; ensure `data/inputs.csv` has valid user IDs
