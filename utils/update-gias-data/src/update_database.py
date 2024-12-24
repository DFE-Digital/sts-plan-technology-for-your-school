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


def _get_connection():
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
            CONNECTION_STRING, attrs_before={1256: token_struct}
        )
        logger.info("Successfully connected to Microsoft Azure SQL Database")
        return connection
    except Exception as ex:
        logger.error("Failed to connect to Azure database", exc_info=ex)
        raise ex


@contextmanager
def _db_cursor():
    connection = _get_connection()
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
    logger.info(
        "Creating #EstablishmentGroup staging table with %s records", len(groups)
    )
    cursor.execute("""CREATE TABLE #EstablishmentGroup (
        uid NVARCHAR(50), 
        groupName NVARCHAR(500), 
        groupType NVARCHAR(200),
        groupTypeCode NVARCHAR(50)
    )""")
    cursor.executemany(
        "INSERT INTO #EstablishmentGroup (uid, groupName, groupType, groupTypeCode) VALUES(?, ?, ?, ?)",
        [
            (row.uid, row.groupName, row.groupType, row.groupTypeCode)
            for _, row in groups.iterrows()
        ],
    )


def _create_temp_links(cursor: Cursor, links: pd.DataFrame) -> None:
    logger.info("Creating #EstablishmentLink staging table with %s records", len(links))
    cursor.execute(
        "CREATE TABLE #EstablishmentLink(uid NVARCHAR(50), establishmentName NVARCHAR(200))"
    )
    cursor.executemany(
        "INSERT INTO #EstablishmentLink (uid, establishmentName) VALUES(?, ?)",
        [(row.uid, row.establishmentName) for _, row in links.iterrows()],
    )


def update_database(groups: pd.DataFrame, links: pd.DataFrame):
    """Use a single transaction to update establishment groups and relationships, commit on success"""
    with _db_cursor() as cursor:
        cursor.fast_executemany = True
        _create_temp_groups(cursor, groups)
        _create_temp_links(cursor, links)
        logger.info(
            "Updating dbo.establishment and dbo.establishmentGroup with staging tables"
        )
        cursor.execute("EXEC dbo.UpdateEstablishmentData")
        logger.info("Update complete")
