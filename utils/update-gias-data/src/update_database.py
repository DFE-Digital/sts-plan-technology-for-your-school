import math
import re
import struct

import numpy as np
import pandas as pd
import pyodbc

from azure import identity
from contextlib import contextmanager

from src.classes import GiasData
from src.utils import get_logger
from src.validation import validate_before_db, validate_against_db

logger = get_logger(__name__)

SQL_COPT_SS_ACCESS_TOKEN = 1256

# DSI test data - must survive every sync
_TEST_GROUP = pd.DataFrame(
    [
        {
            "groupUid": 99999,
            "groupId": None,
            "ukprn": None,
            "groupName": "DSI TEST Multi-Academy Trust (010)",
            "groupStatusCode": "OPEN",
            "groupTypeCode": 6,
        }
    ]
)

_TEST_ESTABLISHMENTS = pd.DataFrame(
    [
        {
            "urn": 900006,
            "establishmentName": "DSI TEST Establishment (001) Community School (01)",
            "establishmentStatusCode": 1,
            "genderCode": 3,
            "localAuthorityCode": 352,
            "phaseCode": 7,
        },
        {
            "urn": 900007,
            "establishmentName": "DSI TEST Establishment (001) Foundation School (05)",
            "establishmentStatusCode": 1,
            "genderCode": 3,
            "localAuthorityCode": 352,
            "phaseCode": 7,
        },
        {
            "urn": 900008,
            "establishmentName": "DSI TEST Establishment (001) Miscellaneous (27)",
            "establishmentStatusCode": 1,
            "genderCode": 3,
            "localAuthorityCode": 352,
            "phaseCode": 7,
        },
    ]
)

_TEST_MEMBERSHIP = pd.DataFrame(
    [
        {"urn": 900006, "groupUid": 99999},
        {"urn": 900007, "groupUid": 99999},
        {"urn": 900008, "groupUid": 99999},
    ]
)


# ---------------------------------------------------------------------------
# Connection helpers
# ---------------------------------------------------------------------------


def _get_connection(connection_string: str):
    if re.search(r"(UID=|User ID=)", connection_string, re.IGNORECASE):
        logger.info("Using standard SQL auth")
        return pyodbc.connect(connection_string)

    logger.info("Using Azure AD token auth")
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
    cursor.fast_executemany = True

    try:
        logger.info("Beginning transaction")
        yield cursor
    except Exception as ex:
        logger.error(
            "Error while executing query, rolling back transaction", exc_info=ex
        )
        connection.rollback()
        raise
    else:
        logger.info("Committing transaction")
        connection.commit()
    finally:
        logger.info("Closing connection")
        cursor.close()
        connection.close()
        logger.info("Connection closed")


# ---------------------------------------------------------------------------
# Parameter conversion helpers
# ---------------------------------------------------------------------------


def _coerce(v):
    """
    Convert a single value to a pyodbc-safe Python type.
    Without this, the DataFrame leaks NumPy/Pandas types into pyodbc parameters.
    """
    try:
        if pd.isna(v):
            return None
    except (TypeError, ValueError):
        pass
    if isinstance(v, float) and math.isnan(v):
        return None
    if isinstance(v, pd.Timestamp):
        return v.date()
    if isinstance(v, np.generic):  # numpy scalar
        return v.item()
    return v


def _params(df: pd.DataFrame) -> list[tuple]:
    return [
        tuple(_coerce(v) for v in row) for row in df.itertuples(index=False, name=None)
    ]


# ---------------------------------------------------------------------------
# Validation
# ---------------------------------------------------------------------------


def _log_proposed_changes(data: GiasData, row_counts_before: dict[str, int]):
    n_est = len(data.establishments)
    n_grp = len(data.establishment_groups)
    n_mem = len(data.group_membership)
    n_map = len(data.links)

    for key, proposed in [
        ("gias.establishment", n_est),
        ("gias.establishmentGroup", n_grp),
        ("gias.groupMembership", n_mem),
        ("gias.links", n_map),
    ]:
        before = row_counts_before.get(key, 0)
        logger.info("%s: %d → %d (%+d)", key, before, proposed, proposed - before)


def _row_count(cur: pyodbc.Cursor, table: str) -> int:
    cur.execute(f"SELECT COUNT(*) FROM {table}")  # noqa: S608 (internal table name)
    results = cur.fetchone()

    if (results is None) or (len(results) == 0):
        return 0

    row_count = results[0]
    return row_count


def _row_counts(cur: pyodbc.Cursor) -> dict[str, int]:
    tables = [
        "gias.establishment",
        "gias.establishmentGroup",
        "gias.groupMembership",
        "gias.links",
    ]
    return {t: _row_count(cur, t) for t in tables}


# ---------------------------------------------------------------------------
# Merge helpers
# ---------------------------------------------------------------------------


def _bulk_insert(cur: pyodbc.Cursor, sql: str, rows: list[tuple], label: str) -> None:
    logger.info("Inserting %d rows into %s", len(rows), label)
    cur.executemany(sql, rows)


def _merge_lookup(
    cur,
    temp: str,
    target: str,
    pk: str,
    pk_type: str,
    cols: list[str],
    df: pd.DataFrame,
) -> None:
    col_defs = f"{pk} {pk_type}, " + ", ".join(f"{c} NVARCHAR(200)" for c in cols)
    set_clause = ", ".join(f"{c} = src.{c}" for c in cols)
    insert_cols = ", ".join([pk] + cols)
    insert_vals = ", ".join([f"src.{pk}"] + [f"src.{c}" for c in cols])
    placeholders = ", ".join(["?"] * (1 + len(cols)))

    cur.execute(f"CREATE TABLE {temp} ({col_defs})")
    _bulk_insert(cur, f"INSERT INTO {temp} VALUES ({placeholders})", _params(df), temp)
    cur.execute(f"""
        MERGE {target} AS tgt
        USING {temp} AS src ON tgt.{pk} = src.{pk}
        WHEN MATCHED THEN UPDATE SET {set_clause}
        WHEN NOT MATCHED THEN INSERT ({insert_cols}) VALUES ({insert_vals});
    """)
    cur.execute(f"DROP TABLE {temp}")


# ---------------------------------------------------------------------------
# Per-table merge functions
# ---------------------------------------------------------------------------


def _merge_establishment_statuses(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    logger.info("Merging establishment status lookup")
    _merge_lookup(
        cur,
        "#EstStatus",
        "gias.establishmentStatus",
        "establishmentStatusCode",
        "INT",
        ["establishmentStatusName"],
        df,
    )
    logger.info("Merged establishment status")


def _merge_genders(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    logger.info("Merging gender lookup")
    _merge_lookup(
        cur, "#Gender", "gias.gender", "genderCode", "INT", ["genderName"], df
    )
    logger.info("Merged gender")


def _merge_group_statuses(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    logger.info("Merging group status lookup")
    _merge_lookup(
        cur,
        "#GrpStatus",
        "gias.groupStatus",
        "groupStatusCode",
        "NVARCHAR(100)",
        ["groupStatusName"],
        df,
    )
    logger.info("Merged group status")


def _merge_group_types(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    logger.info("Merging group type lookup")
    _merge_lookup(
        cur, "#GT", "gias.groupType", "groupTypeCode", "INT", ["groupTypeName"], df
    )
    logger.info("Merged groupType")


def _merge_local_authorities(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    logger.info("Merging local authority lookup")
    _merge_lookup(
        cur,
        "#LA",
        "gias.localAuthority",
        "localAuthorityCode",
        "INT",
        ["localAuthorityName"],
        df,
    )
    logger.info("Merged local authority")


def _merge_phases(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    logger.info("Merging phase lookup")
    _merge_lookup(cur, "#Phase", "gias.phase", "phaseCode", "INT", ["phaseName"], df)
    logger.info("Merged phase")


def _merge_establishments(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    cur.execute("""
        CREATE TABLE #Est (
            urn INT,
            uprn BIGINT,
            ukprn INT,
            establishmentNumber INT,
            establishmentName NVARCHAR(255),
            establishmentStatusCode INT NOT NULL,
            genderCode INT NOT NULL,
            localAuthorityCode INT NOT NULL,
            phaseCode INT NOT NULL
        )
    """)
    _bulk_insert(
        cur,
        "INSERT INTO #Est VALUES (" + ",".join(["?"] * 9) + ")",
        _params(df),
        "#Est",
    )
    cur.execute("""
        MERGE gias.establishment AS tgt
        USING #Est AS src ON tgt.urn = src.urn
        WHEN MATCHED THEN UPDATE SET
            uprn = src.uprn,
            ukprn = src.ukprn,
            establishmentNumber = src.establishmentNumber,
            establishmentName = src.establishmentName,
            establishmentStatusCode = src.establishmentStatusCode,
            genderCode = src.genderCode,
            localAuthorityCode = src.localAuthorityCode,
            phaseCode = src.phaseCode,
            syncedAt = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN INSERT (
            urn,
            uprn,
            ukprn,
            establishmentNumber,
            establishmentName,
            establishmentStatusCode,
            genderCode,
            localAuthorityCode,
            phaseCode
        ) VALUES (
            src.urn,
            src.uprn,
            src.ukprn,
            src.establishmentNumber,
            src.establishmentName,
            src.establishmentStatusCode,
            src.genderCode,
            src.localAuthorityCode,
            src.phaseCode
        );
    """)
    cur.execute("DROP TABLE #Est")
    logger.info("Merged establishments")


def _merge_establishment_groups(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    cur.execute("""
        CREATE TABLE #Grp (
            groupUid INT,
            groupId NVARCHAR(100),
            ukprn INT,
            groupName NVARCHAR(255),
            groupStatusCode NVARCHAR(100),
            groupTypeCode INT
        )
    """)
    _bulk_insert(
        cur,
        "INSERT INTO #Grp VALUES (" + ",".join(["?"] * 6) + ")",
        _params(df),
        "#Grp",
    )
    cur.execute("""
        MERGE gias.establishmentGroup AS tgt
        USING #Grp AS src ON tgt.groupUid = src.groupUid
        WHEN MATCHED THEN UPDATE SET
            groupId = src.groupId,
            ukprn = src.ukprn,
            groupName = src.groupName,
            groupStatusCode = src.groupStatusCode,
            groupTypeCode = src.groupTypeCode,
            syncedAt = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN INSERT (
            groupUid,
            groupId,
            ukprn,
            groupName,
            groupStatusCode,
            groupTypeCode
        ) VALUES (
            src.groupUid,
            src.groupId,
            src.ukprn,
            src.groupName,
            src.groupStatusCode,
            src.groupTypeCode
        );
    """)
    cur.execute("DROP TABLE #Grp")
    logger.info("Merged establishment_groups")


def _replace_group_membership(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    """DELETE current memberships and re-insert from the latest snapshot."""
    cur.execute("""
        CREATE TABLE #Mem (
            urn INT, groupUid INT
        )
    """)
    _bulk_insert(cur, "INSERT INTO #Mem VALUES (?, ?)", _params(df), "#Mem")
    cur.execute("TRUNCATE TABLE gias.groupMembership")
    cur.execute("""
        INSERT INTO gias.groupMembership (urn, groupUid)
        SELECT urn, groupUid FROM #Mem
    """)
    cur.execute("DROP TABLE #Mem")
    logger.info("Replaced group_membership")


def _replace_links(cur: pyodbc.Cursor, df: pd.DataFrame) -> None:
    """DELETE current links and re-insert from the latest snapshot."""
    cur.execute("""
        CREATE TABLE #Link (
            urn INT, linkedUrn INT, linkType VARCHAR(255), dateLinked DATE
        )
    """)
    _bulk_insert(
        cur,
        "INSERT INTO #Link VALUES (?, ?, ?, ?)",
        _params(df),
        "#Link",
    )
    cur.execute("TRUNCATE TABLE gias.links")
    cur.execute("""
        INSERT INTO gias.links (urn, linkedUrn, linkType, dateLinked)
        SELECT urn, linkedUrn, linkType, dateLinked FROM #Link
    """)
    cur.execute("DROP TABLE #Link")
    logger.info("Replaced links")


# ---------------------------------------------------------------------------
# Test-data injection
# ---------------------------------------------------------------------------


def _inject_test_data(data: GiasData) -> GiasData:
    """Merge DSI test rows into the DataFrames before syncing to the DB."""
    # Establishments - only add if not already present in real GIAS data
    real_urns = set(data.establishments["urn"].dropna().astype(int))
    missing = _TEST_ESTABLISHMENTS[~_TEST_ESTABLISHMENTS["urn"].isin(real_urns)]
    missing = missing.copy()

    if not missing.empty:
        # Pad missing columns with None so concat works
        for col in data.establishments.columns:
            if col not in missing.columns:
                missing[col] = None
        data.establishments = pd.concat(
            [data.establishments, missing[data.establishments.columns]],
            ignore_index=True,
        )

    # Groups - always upsert (test group may change name)
    #
    # Note:
    # If a real GIAS group has the same UID as the test group, the test group will not be added,
    # but the real group will be updated with the test group's name and type.
    # This is an acceptable edge case for simplicity of implementation.

    real_guids = set(data.establishment_groups["groupUid"].dropna().astype(int))
    if 99999 not in real_guids:
        data.establishment_groups = pd.concat(
            [data.establishment_groups, _TEST_GROUP], ignore_index=True
        )

    # Membership - always include
    data.group_membership = pd.concat(
        [data.group_membership, _TEST_MEMBERSHIP], ignore_index=True
    ).drop_duplicates(subset=["urn", "groupUid"])

    return data


# ---------------------------------------------------------------------------
# Public entry point
# ---------------------------------------------------------------------------


def update_database(
    data: GiasData,
    connection_string: str,
    skip_validation: bool = False,
) -> None:
    validate_before_db(data, skip_validation)
    data = _inject_test_data(data)

    with _db_cursor(connection_string) as cur:
        counts_before = _row_counts(cur)
        validate_against_db(cur, data, counts_before, skip_validation)
        _log_proposed_changes(data, counts_before)

        # Lookups

        _merge_establishment_statuses(cur, data.establishment_statuses)
        _merge_genders(cur, data.genders)
        _merge_group_statuses(cur, data.group_statuses)
        _merge_group_types(cur, data.group_types)
        _merge_local_authorities(cur, data.local_authorities)
        _merge_phases(cur, data.phases)

        # Core entities
        _merge_establishments(cur, data.establishments)
        _merge_establishment_groups(cur, data.establishment_groups)

        # Child tables
        _replace_group_membership(cur, data.group_membership)
        _replace_links(cur, data.links)

        counts_after = _row_counts(cur)
        for table in counts_after:
            logger.info(
                "%s: %d → %d",
                table,
                counts_before.get(table, 0),
                counts_after[table],
            )
