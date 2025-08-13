# GIAS data updater

## Overview
Establishment groups reside in the `dbo.establishmentGroup` table, which the `dbo.establishment` table maps to by foreign key.

GIAS data is updated daily and there is no API to retrieve it,
so this python project uses playwright to pull down the zip folder of groups and links,
then uses pandas to parse it, and pyodbc to update the SQL database with any changes.
It runs as part of our github actions on a daily schedule.

Follows the guidance [in the Microsoft documentation](https://learn.microsoft.com/en-us/azure/azure-sql/database/azure-sql-python-quickstart?view=azuresql&tabs=windows%2Csql-inter#add-code-to-connect-to-azure-sql-database)
for connecting to the database with pyodbc.

## Local setup

To run the data-updater locally:

1. Install [uv](https://github.com/astral-sh/uv)
2. Install the [Microsoft ODBC Driver](https://learn.microsoft.com/en-us/sql/connect/odbc/linux-mac/install-microsoft-odbc-driver-sql-server-macos?view=azuresqldb-current)
3. Setup your .env file by copying .env.example and filling in the required environment variables:

   | Variable          | Description                                      | Example / Location                              |
   |-------------------|--------------------------------------------------|-------------------------------------------------|
   | CONNECTION_STRING | SQL Connection string for the plan-tech database | Keyvault value for `python--dbconnectionstring` |

4. create a virtual environment and install dependencies:
   ```bash
   make setup
   ```

5. Fetch the GIAS data and update the database with
   ```bash
   make run
   ```

## Linting and formatting

Linting and formatting can be done with:

```bash
make lint
```

## Windows Machines

If you don't have make installed on your machine, you can run the above steps by running the commands in the Makefile directly.

For windows setup this would be:

1. First time setup
   ```bash
   cd utils/update-gias-data
   pip install uv
   uv venv
   .venv/Scripts/activate
   uv sync
   uv run playwright install --with-deps webkit
   ```

2. Running the Project
   ```bash
   uv run main.py
   ```

3. Linting and formatting
   ```bash
   uv run ruff check --fix
   uv run ruff format
   ```
