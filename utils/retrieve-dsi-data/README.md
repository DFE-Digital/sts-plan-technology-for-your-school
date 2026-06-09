# Retrieve DSI User Data

This project reads a CSV file containing a list of DfE Sign-In user IDs, calls the
[DfE Sign-In Public API](https://github.com/DFE-Digital/login.dfe.public-api?tab=readme-ov-file) for
each user ID, and generates both a CSV report and a **T-SQL script** for importing the data into SQL
Server.

---

## Features

- Reads a CSV file containing a `dfeSignInRef` column
- Authenticates with the DSI Public API using JWT (HS256)
- Makes asynchronous API calls to two endpoints per user:
  - `/users/{id}/organisationservices` - Organizations and services
  - `/users/{id}/v2/organisations` - Enhanced organization details (UKPRN, UPIN, provider data)
- Extracts comprehensive user and organization data
- **Generates a T-SQL script** that:
  - Creates a normalized `dsi` schema in SQL Server
  - Defines tables: `Users`, `Organisations`, `UserOrganisationMappings`, `Services`, `Roles`,
    `UserServiceRoles`
  - Inserts all collected data with proper foreign key relationships
  - Includes summary statistics query
- **Validates data integrity** before SQL generation via local SQLite database
- **Optional discovery mode** to find additional users via paginated API endpoint
- **Modular architecture** with separate `lib/` modules for database, API, validation, and reports
- Creates timestamped output files:
  - `dsi-user-data-YYYYMMDD.csv` - Summary CSV report
  - `dsi-user-data-YYYYMMDD.sql` - T-SQL script ready to execute on SQL Server

---

## Setup

1. **Install dependencies**:

   ```bash
   npm install
   ```

2. Create a copy of the `.env.example` file, rename it `.env`, and populate with:
   - `CLIENT_ID` - Your DfE Sign-In service client ID
   - `API_SECRET` - Your DfE Sign-In API secret
   - `API_URL` - The DfE Sign-In API endpoint URL
   - (Optional) `DISCOVERY_MODE=true` - Enable discovery of additional users (default: false)
   - (Optional) `API_BACKOFF_MS=100` - Delay between API calls in milliseconds (default: 100)

   The first three are available from the DSI Manage console for your service.

3. Create a `data` folder (if it doesn't exist) and add an `inputs.csv` file with the following
   structure:

   ```csv
   dfeSignInRef
   user-id-123
   user-id-456
   user-id-789
   ```

4. Run the script:

   ```bash
   node index.js
   ```

---

## Output Files

The script generates output files in the `outputs/` folder with ISO date format (YYYYMMDD):

### 1. `dsi-user-data-YYYYMMDD.csv`

Summary report with:

- `email` - User's email address
- `name` - User's full name (givenName + familyName)
- `user_source` - Either "DfE Sign In" (seed user) or "Discovered" (found via discovery mode)
- `organisation_count` - Number of organizations the user has access to

### 2. `dsi-user-data-YYYYMMDD.sql`

T-SQL script for SQL Server containing:

**Schema Definition:**

- Creates `dsi` schema (if not exists)
- Drops existing tables (for safe re-runs)
- Creates normalized tables:
  - `dsi.Users` - User core data (userId, email, name, status)
  - `dsi.Organisations` - Comprehensive organization data:
    - Identifiers: ID, UKPRN, UPIN, URN, UID, establishment number
    - Details: Name, legal name, category, provider type, source system
    - Dates: opened, closed
    - Contact: address, telephone
    - Provider info: GIAS, PIMS, provider profile data
    - Status tracking: current status, PIMS status, GIAS status
  - `dsi.UserOrganisationMappings` - User-organization relationships
  - `dsi.Services` - Services per organization
  - `dsi.Roles` - Roles within services
  - `dsi.UserServiceRoles` - User's roles in services

**Data Inserts:**

- INSERT statements for all users
- INSERT statements for all organizations (no duplicates)
- Relationships linking users to organizations and services
- Summary statistics query at the end

**Usage:**

```sql
-- Edit this line to match your target database
USE [YourDatabaseName];

-- Run the entire script
-- This will create the schema and populate all tables
```

---

## Data Collected

Per user, the script collects and stores:

| Entity           | Fields                                                                                                                           |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **User**         | userId, email, givenName, familyName, userStatus                                                                                 |
| **Organisation** | id, name, category, URN, UID, UKPRN, UPIN, status, address, legal name, provider types, source system, and 20+ additional fields |
| **Service**      | name, description, organization association                                                                                      |
| **Role**         | name, code, service association                                                                                                  |
| **Mappings**     | User→Org, User→Service→Role relationships                                                                                        |

---

## Data Processing

The application executes 6 sequential phases:

1. **Initialize SQLite database** - Creates local validation database at `data/dsi-YYYYMMDD.db`
2. **Read seed users** - Parses `data/inputs.csv`
3. **Process seed users** - Calls DfE Sign-In API for each user (two endpoints per user)
4. **Discovery mode** (optional) - Finds additional users via paginated `/users` endpoint
5. **Validate database** - Checks for data integrity; stops if validation fails
6. **Generate outputs** - Creates SQL script and CSV report

## Error Handling

- **API failures** for individual users are logged to console; processing continues for other users
- **Validation failures** halt execution and prevent SQL generation (exits with code 1)
- **Data integrity** enforced via SQLite foreign key constraints
- The script is **idempotent**: running it multiple times produces identical results
- SQLite database is **persisted** for inspection and debugging

### When Validation Fails

If data integrity issues are detected:

```
❌ VALIDATION FAILED
  ❌ Found X orphaned user-organization mappings

Outputs NOT generated due to validation errors.
```

Inspect the SQLite database to debug: `sqlite3 data/dsi-YYYYMMDD.db`

---

## Architecture

The application is organized into modular components:

```
lib/
├── api.js       - DfE Sign-In API client with JWT caching
├── db.js        - SQLite database layer (better-sqlite3)
├── validate.js  - Data integrity validation
├── reports.js   - T-SQL and CSV generation
└── utils.js     - Utility functions (UUID, sleep)
```

See [CODE_ENHANCEMENT_REFERENCE.md](CODE_ENHANCEMENT_REFERENCE.md) for detailed architecture
documentation.

---

## Dependencies

- `dotenv` - Environment variable management
- `csv-parser` - CSV file reading
- `axios` - HTTP requests
- `jsonwebtoken` - JWT token generation for API authentication
- `json2csv` - CSV file generation
- `better-sqlite3` - SQLite3 database for local validation layer (v9.2.2+)

---

## Notes

- **Data Protection**: There is a `.gitignore` for this project that ignores any `.csv`, `.db`, and
  `.sql` files to prevent accidental committing of user data.
- **JWT Token**: Valid for 5 minutes; cached for 4.5 minutes to reduce generation overhead.
- **API Backoff**: Configurable via `API_BACKOFF_MS` (default 100ms) to respect rate limits.
- **Discovery Mode**: Can retrieve all users in your service scope beyond the seed list. CPU and API
  intensive for large deployments.
- **SQLite Database**: Persisted locally for validation and debugging. Delete after use or archive
  for audit trail.
- **SQL Server Compatibility**: Generated SQL is compatible with SQL Server 2016+.
- **Idempotent Script**: The generated T-SQL script drops and recreates tables, making it safe to
  run multiple times.
- **Data Security**: Secure handling and transfer of CSV/SQL output files should follow DfE data
  protection policies; refer to the
  [Data Protection Hub](https://educationgovuk.sharepoint.com/sites/lvewp00158).

## Troubleshooting

### "VALIDATION FAILED"

This means data integrity checks found issues. Options:

1. Inspect the SQLite database: `sqlite3 data/dsi-YYYYMMDD.db`
2. Review console output for specific errors
3. Check that `data/inputs.csv` contains valid user IDs

### "No outputs generated"

Possible causes:

1. API authentication failed (check CLIENT_ID, API_SECRET, API_URL)
2. No users found in seed CSV
3. Validation failed (see above)

Check the console output for error messages.

---

## See also

- [Code Enhancement Reference](CODE_ENHANCEMENT_REFERENCE.md) - Detailed module documentation
- [DSI SQL Implementation Guide](DSI_SQL_IMPLEMENTATION_GUIDE.md) - SQL schema and queries
- [Utils overview](../README.md)
