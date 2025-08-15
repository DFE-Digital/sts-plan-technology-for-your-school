import os
import struct
from contextlib import contextmanager
from logging import getLogger

import pandas as pd
import pyodbc
from azure import identity
from dotenv import load_dotenv
from pyodbc import Cursor

load_dotenv()
logger = getLogger(__name__)

CONNECTION_STRING = os.getenv("CONNECTION_STRING")
SQL_COPT_SS_ACCESS_TOKEN = 1256

THRESHOLD_MIN_RECORDS = 1000
THRESHOLD_SUBSTANTIAL_CHANGE = 1000

test_links = pd.DataFrame(
    [
        {
            "groupUid": "99999",
            "establishmentName": "DSI TEST Establishment (001) Community School (01)",
            "urn": 2,
        },
        {
            "groupUid": "99999",
            "establishmentName": "DSI TEST Establishment (001) Miscellaneous (27)",
            "urn": 18,
        },
        {
            "groupUid": "99999",
            "establishmentName": "DSI TEST Establishment (001) Foundation School (05)",
            "urn": 5,
        },
    ]
)

test_groups = pd.DataFrame(
    [
        {
            "uid": "99999",
            "groupName": "DSI TEST Multi-Academy Trust (010)",
            "groupType": "Multi-academy trust",
            "groupStatus": "Open",
        }
    ]
)


def _get_connection(connection_string: str):
    """Fetch a token for accessing the database and connect"""
    try:
        credential = identity.DefaultAzureCredential(
            exclude_interactive_browser_credential=True
        )
        token = credential.get_token("https://database.windows.net/.default")
        token_bytes = token.token.encode("UTF-16-LE")
        token_struct = struct.pack(
            f"<I{len(token_bytes)}s", len(token_bytes), token_bytes
        )
        connection = pyodbc.connect(
            connection_string, attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct}
        )
        logger.info("Successfully connected to Microsoft Azure SQL Database")
        return connection
    except Exception as ex:
        logger.error("Failed to connect to Azure database", exc_info=ex)
        raise ex


@contextmanager
def _db_cursor(connection_string: str):
    connection = _get_connection(connection_string)
    cursor = connection.cursor()
    try:
        logger.info("Beginning transaction")
        yield cursor
    except Exception as ex:
        logger.error("Error while executing query, rolling back", exc_info=ex)
        connection.rollback()
    else:
        logger.info("Committing transaction")
        connection.commit()
    finally:
        logger.info("Closing connection")
        cursor.close()
        connection.close()


def _create_temp_groups(cursor: Cursor, groups: pd.DataFrame) -> None:
    """Create temp table for holding establishment groups"""
    logger.info(
        "Creating #EstablishmentGroup staging table with %s records", len(groups)
    )
    cursor.execute("""CREATE TABLE #EstablishmentGroup (
        uid NVARCHAR(50),
        groupName NVARCHAR(200),
        groupType NVARCHAR(200),
        groupStatus NVARCHAR(200)
    )""")
    cursor.executemany(
        "INSERT INTO #EstablishmentGroup (uid, groupName, groupType, groupStatus) VALUES(?, ?, ?, ?)",
        [
            (row.uid, row.groupName, row.groupType, row.groupStatus)
            for _, row in groups.iterrows()
        ],
    )


def _create_temp_links(cursor: Cursor, links: pd.DataFrame) -> None:
    """Create temp table for holding links between establishment groups and establishments"""
    logger.info("Creating #EstablishmentLink staging table with %s records", len(links))
    cursor.execute(
        "CREATE TABLE #EstablishmentLink(groupUid NVARCHAR(50), establishmentName NVARCHAR(200), urn INT NOT NULL)"
    )
    cursor.executemany(
        "INSERT INTO #EstablishmentLink (groupUid, establishmentName, urn) VALUES(?, ?, ?)",
        [(row.groupUid, row.establishmentName, row.urn) for _, row in links.iterrows()],
    )


def _validate_required_tables_exist(cursor: Cursor) -> bool:
    """
    Check if the required tables exist in the database.
    Returns True if both tables exist, False otherwise.
    """
    cursor.execute("""
        SELECT COUNT(*) AS table_count
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME IN ('establishmentLink', 'establishmentGroup')
        AND TABLE_SCHEMA = 'dbo'
    """)
    table_count = cursor.fetchone()[0]

    if table_count < 2:
        logger.error(
            "Required tables (establishmentLink and/or establishmentGroup) not found in the database"
        )
        logger.error("This is likely not the correct database for this operation")
        logger.error("No data will be updated")
        return False
    else:
        logger.info("Required tables found in the database, proceeding with update")
        return True


def _get_table_row_counts(cursor: Cursor, tables: list[str]) -> dict[str, int]:
    """
    Get the row counts for the specified tables.
    Returns a dictionary with table names as keys and row counts as values.
    """
    counts = {}
    for table in tables:
        try:
            cursor.execute(f"SELECT COUNT(*) FROM {table}")
            count = cursor.fetchone()[0]
            counts[table] = count
        except Exception as ex:
            logger.error(f"Error getting row count for {table}: {str(ex)}")
            counts[table] = -1
    return counts


def _get_database_name(cursor: Cursor) -> str:
    """Get the name of the current database"""
    cursor.execute("SELECT DB_NAME() AS current_database")
    return cursor.fetchone()[0]


def _execute_stored_procedure(cursor: Cursor, procedure_name: str) -> bool:
    """Execute a stored procedure and return success status"""
    try:
        logger.info(f"Executing stored procedure: {procedure_name}")
        cursor.execute(f"EXEC {procedure_name}")
        logger.info("Successfully executed stored procedure")
        return True
    except Exception as ex:
        logger.error(f"Error executing stored procedure: {str(ex)}")
        raise


def _prepare_combined_test_and_real_data(
    groups: pd.DataFrame, links: pd.DataFrame
) -> tuple[pd.DataFrame, pd.DataFrame]:
    """Combine the provided data with test data"""
    combined_links = pd.concat([links, test_links], ignore_index=True)
    combined_groups = pd.concat([groups, test_groups], ignore_index=True)
    return combined_groups, combined_links


def _validate_data_for_update(
    combined_groups: pd.DataFrame,
    combined_links: pd.DataFrame,
    row_counts_before: dict[str, int],
    skip_gias_validation: bool = False,
) -> bool:
    """
    Validate that the data to be updated meets all requirements.
    Returns True if data is valid, False otherwise.

    Args:
        combined_groups: DataFrame containing establishment group data
        combined_links: DataFrame containing establishment link data
        row_counts_before: Dictionary with current row counts
        skip_gias_validation: If True, bypasses GIAS data validation checks for abnormal data
    """
    # If skip validation is enabled, bypass validation
    if skip_gias_validation:
        logger.warning(
            "GIAS data validation checks bypassed. Proceeding regardless of data quality concerns."
        )
        return True

    # Check for empty dataframes
    if combined_groups.empty or combined_links.empty:
        logger.warning("No data to update. Skipping database update.")
        return False

    # Validate there are not a suspicious number of records
    if (
        len(combined_groups) < THRESHOLD_MIN_RECORDS
        or len(combined_links) < THRESHOLD_MIN_RECORDS
    ):
        logger.warning(
            f"Exiting early due to suspiciously low number of group and/or link records: "
            f"{len(combined_groups)} groups, {len(combined_links)} links"
        )
        return False

    # If there is a significant _change_ in the number of records, log a warning and exit
    current_establishment_group_count = row_counts_before.get(
        "dbo.establishmentGroup", 0
    )
    proposed_establishment_group_count = len(combined_groups)
    current_establishment_link_count = row_counts_before.get("dbo.establishmentLink", 0)
    proposed_establishment_link_count = len(combined_links)

    # Log the proposed changes
    logger.info(
        f"Current counts: "
        f"{current_establishment_group_count} groups, {current_establishment_link_count} links"
    )
    logger.info(
        f"Proposed counts: "
        f"{proposed_establishment_group_count} groups ({proposed_establishment_group_count - current_establishment_group_count:+}), "
        f"{proposed_establishment_link_count} links ({proposed_establishment_link_count - current_establishment_link_count:+})"
    )

    if (
        abs(proposed_establishment_group_count - current_establishment_group_count) > THRESHOLD_SUBSTANTIAL_CHANGE
        or abs(proposed_establishment_link_count - current_establishment_link_count) > THRESHOLD_SUBSTANTIAL_CHANGE
    ):
        logger.warning(
            f"Exiting early due to a suspiciously large change in establishment group ({current_establishment_group_count} -> {proposed_establishment_group_count}) and/or link ({current_establishment_link_count} -> {proposed_establishment_link_count}) counts."
        )
        return False

    # All validations passed
    logger.info("GIAS data validation passed, proceeding with update.")
    return True


def update_database(
    groups: pd.DataFrame,
    links: pd.DataFrame,
    connection_string: str,
    skip_gias_validation: bool = False,
):
    """
    Use a single transaction to update establishment groups and relationships, commit on success

    Args:
        groups: DataFrame containing establishment group data
        links: DataFrame containing establishment link data
        connection_string: Database connection string
        skip_gias_validation: If True, bypasses GIAS data validation checks for abnormal or suspicious data
    """
    with _db_cursor(connection_string) as cursor:
        cursor.fast_executemany = True

        try:
            # Validate required tables exist
            if not _validate_required_tables_exist(cursor):
                return

            # Get row counts before update
            tables_to_check = ["dbo.establishmentGroup", "dbo.establishmentLink"]
            row_counts_before = _get_table_row_counts(cursor, tables_to_check)
            logger.info("Row counts before update:")
            for table, count in row_counts_before.items():
                logger.info(f"  - {table}: {count}")

            # Prepare and load data into temp tables
            combined_groups, combined_links = _prepare_combined_test_and_real_data(
                groups, links
            )

            # Validate the proposed data updates prior to execution
            if not _validate_data_for_update(
                combined_groups, combined_links, row_counts_before, skip_gias_validation
            ):
                return

            # Create temporary tables for staging data
            _create_temp_groups(cursor, combined_groups)
            _create_temp_links(cursor, combined_links)

            logger.info(
                "Updating dbo.establishmentLink and dbo.establishmentGroup with staging tables"
            )

            # Execute the stored procedure to update the data
            # Note the sproc appears to handle inserts, updates, and deletes
            # (e.g. it will delete records that are no longer present GIAS data)
            # ref: src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2025/20250403_1100_AddEstablishmentGroupTable.sql
            # Given this, it is not required to do so within python code.
            stored_procedure_name = "dbo.UpdateEstablishmentData"
            if not _execute_stored_procedure(cursor, stored_procedure_name):
                logger.error(
                    f"Stored procedure {stored_procedure_name} failed to execute"
                )
                return

            # Get row counts after update
            row_counts_after = _get_table_row_counts(cursor, tables_to_check)
            logger.info("Row counts after update:")
            for table, count in row_counts_after.items():
                logger.info(f"  - {table}: {count}")

            # Log the changes
            for table in tables_to_check:
                before = row_counts_before.get(table, 0)
                after = row_counts_after.get(table, 0)
                logger.info(f"Net change in {table}: {after - before}")

        except Exception as ex:
            if "row_counts_before" not in locals():
                logger.error(f"Error during database validation: {str(ex)}")
            else:
                logger.error(f"Error during data update: {str(ex)}")
            raise
