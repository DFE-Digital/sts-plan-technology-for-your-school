"""Pre-update validation for GIAS data.

Two phases:
- validate_before_db(): structural + heuristic checks on the extracted DataFrames.
  Run before opening the DB connection, on the raw GIAS data (before test
  injection) so test rows can't mask real problems.
- validate_against_db(): pre-flight + change-threshold checks. Run inside the
  open transaction once row counts are known.

Two tiers:
- Structural: NA in NOT NULL columns, broken FK references. Always enforced,
  even when skip_gias_validation=True - these catch issues that would crash
  the DB insert mid-transaction anyway.
- Heuristic: per-table minimum row counts and substantial-change limits.
  Skippable when expecting an unusual shape (term changeover, known GIAS
  reorganisation, etc.).
"""

from __future__ import annotations

import pandas as pd
import pyodbc

from src.classes import GiasData
from src.utils import get_logger

logger = get_logger(__name__)


# ---------------------------------------------------------------------------
# Thresholds - tune these after a few days of real runs.
# ---------------------------------------------------------------------------

# Minimum row count per DataFrame. Set to roughly 50% of typical observed
# value so a half-finished download or upstream schema break trips this.
MIN_RECORDS: dict[str, int] = {
    "establishments": 20_000,
    "establishment_groups": 4_000,
    "group_membership": 8_000,
    "establishment_statuses": 4,
    "genders": 3,
    "group_statuses": 2,
    "group_types": 5,
    "local_authorities": 100,
    "phases": 5,
}

# Per-table daily change limit. Allowed move is the larger of:
#   - MAX_CHANGE_PCT of the current row count, or
#   - MIN_CHANGE_ABS records absolute.
# Small absolute moves are always fine; large percent moves on a small table
# aren't.
MAX_CHANGE_PCT = 0.10
MIN_CHANGE_ABS = 500

REQUIRED_TABLES: tuple[str, ...] = (
    "gias.establishment",
    "gias.establishmentGroup",
    "gias.groupMembership",
    "gias.establishmentStatus",
    "gias.gender",
    "gias.groupStatus",
    "gias.groupType",
    "gias.links",
    "gias.localAuthority",
    "gias.phase",
)


class ValidationError(Exception):
    """Raised when proposed data fails validation."""


# ---------------------------------------------------------------------------
# Structural checks - always run, even with skip_gias_validation=True.
# ---------------------------------------------------------------------------


def _check_no_nulls(df: pd.DataFrame, cols: list[str], label: str) -> None:
    for col in cols:
        n = int(df[col].isna().sum())
        if n:
            raise ValidationError(
                f"{label}: {n} NA values in required column {col!r} "
                "(would fail NOT NULL insert)."
            )


def _check_fk(
    child: pd.DataFrame,
    child_col: str,
    parent: pd.DataFrame,
    parent_col: str,
    label: str,
) -> None:
    valid = set(parent[parent_col].dropna())
    orphan_mask = child[child_col].notna() & ~child[child_col].isin(valid)
    n_orphan = int(orphan_mask.sum())
    if n_orphan:
        sample = child.loc[orphan_mask, child_col].head(5).tolist()
        raise ValidationError(
            f"{label}: {n_orphan} rows reference {child_col} values not in "
            f"{parent_col} (e.g. {sample})."
        )


def validate_structural_integrity(data: GiasData) -> None:
    """NA in NOT NULL columns, FK references that don't resolve."""
    _check_no_nulls(
        data.establishments,
        [
            "urn",
            "establishmentName",
            "establishmentStatusCode",
            "genderCode",
            "localAuthorityCode",
            "phaseCode",
        ],
        "establishments",
    )
    _check_no_nulls(
        data.establishment_groups,
        ["groupUid", "groupName", "groupStatusCode", "groupTypeCode"],
        "establishment_groups",
    )
    _check_no_nulls(
        data.group_membership,
        ["urn", "groupUid"],
        "group_membership",
    )
    _check_no_nulls(
        data.links,
        ["urn", "linkedUrn"],
        "links",
    )

    fk_checks = [
        # child, child_col, parent, parent_col, label
        (
            data.establishments,
            "establishmentStatusCode",
            data.establishment_statuses,
            "establishmentStatusCode",
            "establishments -> establishment_statuses",
        ),
        (
            data.establishments,
            "genderCode",
            data.genders,
            "genderCode",
            "establishments -> genders",
        ),
        (
            data.establishments,
            "localAuthorityCode",
            data.local_authorities,
            "localAuthorityCode",
            "establishments -> local_authorities",
        ),
        (
            data.establishments,
            "phaseCode",
            data.phases,
            "phaseCode",
            "establishments -> phases",
        ),
        (
            data.establishment_groups,
            "groupStatusCode",
            data.group_statuses,
            "groupStatusCode",
            "establishment_groups -> group_statuses",
        ),
        (
            data.establishment_groups,
            "groupTypeCode",
            data.group_types,
            "groupTypeCode",
            "establishment_groups -> group_types",
        ),
        (
            data.group_membership,
            "urn",
            data.establishments,
            "urn",
            "group_membership -> establishments",
        ),
        (
            data.group_membership,
            "groupUid",
            data.establishment_groups,
            "groupUid",
            "group_membership -> establishment_groups",
        ),
        (
            data.links,
            "urn",
            data.establishments,
            "urn",
            "links -> establishments",
        ),
        (
            data.links,
            "linkedUrn",
            data.establishments,
            "urn",
            "links -> establishments",
        ),
    ]
    for child, child_col, parent, parent_col, label in fk_checks:
        _check_fk(child, child_col, parent, parent_col, label)

    logger.info("Structural integrity OK")


# ---------------------------------------------------------------------------
# Heuristic checks - skippable.
# ---------------------------------------------------------------------------


def validate_data_quality(data: GiasData) -> None:
    """Each DataFrame meets minimum size expectations."""
    sizes = {
        "establishments": len(data.establishments),
        "establishment_groups": len(data.establishment_groups),
        "group_membership": len(data.group_membership),
        "establishment_statuses": len(data.establishment_statuses),
        "genders": len(data.genders),
        "group_statuses": len(data.group_statuses),
        "group_types": len(data.group_types),
        "local_authorities": len(data.local_authorities),
        "phases": len(data.phases),
    }
    for name, n in sizes.items():
        if n == 0:
            raise ValidationError(f"{name} is empty - refusing to update.")
        threshold = MIN_RECORDS.get(name, 1)
        if n < threshold:
            raise ValidationError(
                f"{name} has {n} rows, below threshold of {threshold}. "
                "GIAS data may be truncated."
            )
    logger.info("Data quality OK: %s", sizes)


def validate_changes_within_thresholds(
    data: GiasData,
    counts_before: dict[str, int],
) -> None:
    """No core table moves by more than the configured threshold."""
    proposed = {
        "gias.establishment": len(data.establishments),
        "gias.establishmentGroup": len(data.establishment_groups),
        "gias.groupMembership": len(data.group_membership),
    }
    for table, after in proposed.items():
        before = counts_before.get(table, 0)
        if before == 0:
            logger.info("%s: %d (initial load, no before count)", table, after)
            continue
        delta = after - before
        threshold = max(int(before * MAX_CHANGE_PCT), MIN_CHANGE_ABS)
        if abs(delta) > threshold:
            raise ValidationError(
                f"{table}: change of {delta:+d} exceeds threshold "
                f"+/-{threshold} ({before} -> {after}). "
                "Re-run with --skip-validation if this is expected."
            )
        logger.info(
            "%s: %d -> %d (%+d), within +/-%d",
            table,
            before,
            after,
            delta,
            threshold,
        )


# ---------------------------------------------------------------------------
# DB pre-flight - run inside an open cursor.
# ---------------------------------------------------------------------------


def validate_required_tables(cur: pyodbc.Cursor) -> None:
    placeholders = ", ".join(["?"] * len(REQUIRED_TABLES))
    cur.execute(
        f"""
        SELECT s.name + '.' + t.name
        FROM sys.tables t
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE s.name + '.' + t.name IN ({placeholders})
        """,
        REQUIRED_TABLES,
    )
    found = {row[0] for row in cur.fetchall()}
    missing = set(REQUIRED_TABLES) - found
    if missing:
        raise ValidationError(
            f"Required tables not found: {sorted(missing)}. "
            "Wrong database, or schema not deployed."
        )
    logger.info("All %d required tables present", len(REQUIRED_TABLES))


# ---------------------------------------------------------------------------
# Orchestration
# ---------------------------------------------------------------------------


def validate_before_db(data: GiasData, skip_validation: bool = False) -> None:
    """Run pre-DB validation. Structural checks always; quality is skippable."""
    validate_structural_integrity(data)
    try:
        validate_data_quality(data)
    except ValidationError as ex:
        if skip_validation:
            logger.warning("Data quality check skipped: %s", ex)
        else:
            raise


def validate_against_db(
    cur: pyodbc.Cursor,
    data: GiasData,
    counts_before: dict[str, int],
    skip_validation: bool = False,
) -> None:
    """Run DB-aware validation. Required-tables always; change limits skippable."""
    validate_required_tables(cur)
    try:
        validate_changes_within_thresholds(data, counts_before)
    except ValidationError as ex:
        if skip_validation:
            logger.warning("Change-threshold check skipped: %s", ex)
        else:
            raise
