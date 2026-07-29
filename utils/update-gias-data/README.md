# GIAS data updater

## Overview

This Python project automatically updates GIAS (Get Information About Schools) establishment data in
the plan-tech database. It:

1. **Fetches data** from the public EDUBASE API (`https://ea-edubase-api-prod.azurewebsites.net`)
2. **Extracts and transforms** the CSV data using pandas
3. **Validates** the data for structural issues and unusual patterns
4. **Updates** the SQL database with any changes using pyodbc

Establishment groups are stored in `dbo.establishmentGroup` and `dbo.establishmentLink` tables. The
process includes comprehensive validation to catch data issues before they reach the database.

The updater runs daily as part of GitHub Actions (see workflows: `update-gias-data-scheduled.yml`,
`update-gias-data-manual.yml`).

## Local setup

To run the data-updater locally:

1. Install [uv](https://github.com/astral-sh/uv).

2. Install the
   [Microsoft ODBC Driver](https://learn.microsoft.com/en-us/sql/connect/odbc/linux-mac/install-microsoft-odbc-driver-sql-server-macos?view=azuresqldb-current).

3. Set up your `.env` file by copying `.env.example` and filling in the required environment
   variable.

   | Variable                                 | Description                                      | Example / Location                              |
   | ---------------------------------------- | ------------------------------------------------ | ----------------------------------------------- |
   | `CONNECTION_STRING_PLANTECH_GIAS_UPDATE` | SQL Connection string for the plan-tech database | Keyvault value for `python--dbconnectionstring` |

4. Spin up the docker container for the database

   ```bash
   docker compose up -d
   ```

5. Create a virtual environment and install dependencies. If you don't have Make installed see
   [Running without Make](#running-without-make).

   ```bash
   cdutils/update-gias-data
   make setup
   ```

6. Run the GIAS data update:

   ```bash
   make run
   ```

   Or with validation checks disabled:

   ```bash
   uv run main.py --skip-validation
   ```

## Linting and formatting

Linting and formatting can be done with:

```bash
make lint
```

## Data validation

The updater includes comprehensive validation to detect data issues before updating the database.
Validation includes:

- **Structural checks**: Ensures required columns are populated and foreign key relationships are
  valid
- **Heuristic checks**: Verifies data meets minimum row count expectations and doesn't have
  unexpectedly large changes

### Skipping validation

Use the `--skip-validation` flag when you expect unusual data patterns (e.g., during term changeover
or known GIAS reorganizations):

```bash
uv run main.py --skip-validation
```

**Note**: Structural checks always run to prevent database errors. Only heuristic checks are
skipped.

## Running without Make

If you don't have make installed on your machine, you can run the above steps by running the
following commands (which are basically the same as in the Makefile).

### First time setup

Ensure you have `uv` on your system.

```bash
pip install uv
```

### Use

```bash
cd utils/update-gias-data
uv venv
source .venv/Scripts/activate
uv sync
```

### Running the project

```bash
uv run main.py
```

Or with validation checks disabled:

```bash
uv run main.py --skip-validation
```

### Linting and formatting

```bash
uv run ruff check --fix
uv run ruff format
```

## See also

- [Utils overview](../README.md)
- [GitHub Actions workflows](../../.github/README.md) — `update-gias-data-scheduled.yml`,
  `update-gias-data-manual.yml`
- [SQL data layer](../../src/Dfe.PlanTech.Data.Sql/README.md) — `dbo.establishmentGroup` and
  `dbo.establishmentLink` tables
- [ADR 0042 — GIAS data refresh](../../docs/architecture-decision-record/0042-gias-data-refresh.md)
- [ADR 0045 — GIAS data API improvements](../../docs/architecture-decision-record/0045-gias-data-api-improvements.md)
