import pandas as pd

from logging import getLogger

from src.classes import GiasData
from src.utils import get_latest_file_path, read_dataframe

logger = getLogger(__name__)


def _coerce_int(series: pd.Series) -> pd.Series:
    stripped = series.astype(str).str.strip()
    result = pd.to_numeric(stripped, errors="coerce").astype("Int64")

    # Preserve the original index to avoid alignment issues
    result.index = series.index
    return result


def _lookup(
    df: pd.DataFrame,
    code_col: str,
    *name_cols: str,
    required: list[str] | None = None,
    blank_replacement: str | None = None,
) -> pd.DataFrame:
    """Extract a de-duplicated lookup DataFrame.

    required: columns that must be non-null and non-blank. Defaults to all columns,
    which is correct for every lookup table whose name columns are NOT NULL in the schema.
    Pass an explicit list for tables with non-nullable columns.

    blank_replacement: if not None, replace blank strings in name columns with this value.
    This is useful for lookups where the name column is nullable but the code column isn't,
    so blank name is effectively a separate "unknown" category rather than just missing data.
    """
    cols = [code_col, *name_cols]

    if required is not None and not set(required).issubset(cols):
        raise ValueError(
            "Required columns %s must exist in the dataframe",
            required,
        )

    out = df[cols].copy()
    null_mask = out[list(name_cols)].isna().any(axis=1)
    unique_codes = out.loc[null_mask, code_col].unique()

    if len(unique_codes) > 1:
        raise ValueError(
            "NA values found in name columns %s with multiple different codes: %s. Cannot proceed.",
            name_cols,
            sorted(unique_codes),
        )

    if len(unique_codes) == 1 and len(name_cols) == 1:
        logger.info(
            "NA values found in column %s, but all have the same code %s. Name value will be replaced with '%s'.",
            name_cols[0],
            unique_codes[0],
            blank_replacement,
        )

        out[name_cols[0]] = out[name_cols[0]].fillna(blank_replacement)

    # Get rows where required columns contain NA values, if any
    if required is not None:
        required_null_mask = out[required].isna().any(axis=1)
        if required_null_mask.any():
            null_rows = out[required][required_null_mask]
            raise ValueError(
                f"NA values found in required lookup columns {required}. Cannot proceed."
            )

    return out.drop_duplicates(subset=[code_col]).reset_index(drop=True)


# ---------------------------------------------------------------------------
# Public entry point
# ---------------------------------------------------------------------------


def extract_gias_data() -> GiasData:
    """Parse the four GIAS CSV files and return normalised DataFrames."""

    edu = read_dataframe(get_latest_file_path("edubasealldata"))
    grp = read_dataframe(get_latest_file_path("allgroupsdata"))
    lnk = read_dataframe(get_latest_file_path("alllinksdata"))

    logger.info(
        "Loaded CSVs - alldata:%d groupsdata:%d linksdata:%d\n",
        len(edu),
        len(grp),
        len(lnk),
    )

    logger.info("Extracting data")
    establishments = _establishments(edu)
    establishment_groups = _establishment_groups(grp)
    group_membership = _group_membership(lnk)

    establishment_statuses = _establishment_statuses(edu)
    genders = _genders(edu)
    group_statuses = _group_statuses(grp)
    group_types = _group_types(grp)
    local_authorities = _local_authorities(edu)
    phases = _phases(edu)

    logger.info("Extracted data - establishments:%d", len(establishments))
    logger.info("Extracted data - establishmentGroups:%d", len(establishment_groups))
    logger.info("Extracted data - groupMembership:%d", len(group_membership))

    logger.info(
        "Extracted data - establishmentStatuses:%d", len(establishment_statuses)
    )
    logger.info("Extracted data - genders:%d", len(genders))
    logger.info("Extracted data - groupStatuses:%d", len(group_statuses))
    logger.info("Extracted data - groupTypes:%d", len(group_types))
    logger.info("Extracted data - localAuthorities:%d", len(local_authorities))
    logger.info("Extracted data - phases:%d\n", len(phases))

    valid_urns = set(establishments["urn"].dropna())
    valid_group_uids = set(establishment_groups["groupUid"].dropna())

    orphaned_urn = group_membership["urn"].notna() & ~group_membership["urn"].isin(
        valid_urns
    )
    orphaned_uid = group_membership["groupUid"].notna() & ~group_membership[
        "groupUid"
    ].isin(valid_group_uids)

    warnings = 0
    if orphaned_urn.any():
        logger.warning(
            "Dropping %d URNs not present in establishments from groupMembership",
            int(orphaned_urn.sum()),
        )
        warnings += 1

    if orphaned_uid.any():
        logger.warning(
            "Dropping %d Group UIDs not present in establishmentGroups from groupMembership",
            int(orphaned_uid.sum()),
        )
        warnings += 1

    if warnings > 0:
        print()

    group_membership = group_membership[~orphaned_urn & ~orphaned_uid].reset_index(
        drop=True
    )

    return GiasData(
        establishments=establishments,
        establishment_groups=establishment_groups,
        group_membership=group_membership,
        establishment_statuses=establishment_statuses,
        group_statuses=group_statuses,
        group_types=group_types,
        genders=genders,
        local_authorities=local_authorities,
        phases=phases,
    )


# ---------------------------------------------------------------------------
# Lookup tables - establishments
# ---------------------------------------------------------------------------


def _establishment_statuses(edu: pd.DataFrame) -> pd.DataFrame:
    out = _lookup(
        edu,
        "EstablishmentStatus (code)",
        "EstablishmentStatus (name)",
        required=["EstablishmentStatus (name)"],
        blank_replacement="Unknown",
    )
    out.columns = ["establishmentStatusCode", "establishmentStatusName"]
    out["establishmentStatusCode"] = _coerce_int(out["establishmentStatusCode"])
    return out.dropna(subset=["establishmentStatusCode"]).reset_index(drop=True)


def _genders(edu: pd.DataFrame) -> pd.DataFrame:
    out = _lookup(
        edu,
        "Gender (code)",
        "Gender (name)",
        required=["Gender (name)"],
        blank_replacement="Unknown",
    )
    out.columns = ["genderCode", "genderName"]
    out["genderCode"] = _coerce_int(out["genderCode"])
    return out.dropna(subset=["genderCode"]).reset_index(drop=True)


def _local_authorities(edu: pd.DataFrame) -> pd.DataFrame:
    out = _lookup(
        edu,
        "LA (code)",
        "LA (name)",
        required=["LA (name)"],
        blank_replacement="Unknown",
    )
    out.columns = ["localAuthorityCode", "localAuthorityName"]
    out["localAuthorityCode"] = _coerce_int(out["localAuthorityCode"])
    return out.dropna(subset=["localAuthorityCode"]).reset_index(drop=True)


def _phases(edu: pd.DataFrame) -> pd.DataFrame:
    out = _lookup(
        edu,
        "PhaseOfEducation (code)",
        "PhaseOfEducation (name)",
        required=["PhaseOfEducation (name)"],
        blank_replacement="Unknown",
    )
    out.columns = ["phaseCode", "phaseName"]
    out["phaseCode"] = _coerce_int(out["phaseCode"])
    return out.dropna(subset=["phaseCode"]).reset_index(drop=True)


# ---------------------------------------------------------------------------
# Lookup tables - groups
# ---------------------------------------------------------------------------


def _group_statuses(grp: pd.DataFrame) -> pd.DataFrame:
    out = _lookup(
        grp,
        "Group Status (code)",
        "Group Status",
        required=["Group Status"],
        blank_replacement="Unknown",
    )
    out.columns = ["groupStatusCode", "groupStatusName"]
    return out.dropna(subset=["groupStatusCode"]).reset_index(drop=True)


def _group_types(grp: pd.DataFrame) -> pd.DataFrame:
    out = _lookup(
        grp,
        "Group Type (code)",
        "Group Type",
        required=["Group Type"],
        blank_replacement="Unknown",
    )
    out.columns = ["groupTypeCode", "groupTypeName"]
    return out.dropna(subset=["groupTypeCode"]).reset_index(drop=True)


# ---------------------------------------------------------------------------
# Core entities
# ---------------------------------------------------------------------------


def _establishments(edu: pd.DataFrame) -> pd.DataFrame:
    cols = {
        "URN": "urn",
        "UPRN": "uprn",
        "UKPRN": "ukprn",
        "EstablishmentNumber": "establishmentNumber",
        "EstablishmentName": "establishmentName",
        "EstablishmentStatus (code)": "establishmentStatusCode",
        "Gender (code)": "genderCode",
        "LA (code)": "localAuthorityCode",
        "PhaseOfEducation (code)": "phaseCode",
    }
    out = edu[list(cols)].rename(columns=cols).copy()

    for col in (
        "urn",
        "uprn",
        "ukprn",
        "establishmentNumber",
        "establishmentStatusCode",
        "genderCode",
        "localAuthorityCode",
        "phaseCode",
    ):
        out[col] = _coerce_int(out[col])

    # Count rows with a blank establishment name and warn if any are found
    blank_name_count = (out["establishmentName"].str.strip() == "").sum()
    if blank_name_count > 0:
        raise ValueError(
            "Found %d rows with blank establishment name. Cannot proceed.",
            blank_name_count,
        )

    # Zero → None for non-URN numeric columns
    for col in ["uprn", "ukprn", "establishmentNumber"]:
        out[col] = out[col].where(out[col] > 0, other=None)

    return out.dropna(subset=["urn"]).reset_index(drop=True)


def _establishment_groups(grp: pd.DataFrame) -> pd.DataFrame:
    cols = {
        "Group UID": "groupUid",
        "Group ID": "groupId",
        "UKPRN": "ukprn",
        "Group Name": "groupName",
        "Group Status (code)": "groupStatusCode",
        "Group Type (code)": "groupTypeCode",
    }
    out = grp[list(cols)].rename(columns=cols).copy()

    int_cols = ["groupUid", "groupId", "ukprn", "groupTypeCode"]
    for col in int_cols:
        out[col] = _coerce_int(out[col])

    return out.dropna(subset=["groupUid", "groupName"]).reset_index(drop=True)


def _group_membership(lnk: pd.DataFrame) -> pd.DataFrame:
    memberships = lnk[["URN", "Group UID"]].copy()
    memberships.columns = ["urn", "groupUid"]
    memberships["urn"] = _coerce_int(memberships["urn"])
    memberships["groupUid"] = _coerce_int(memberships["groupUid"])
    memberships = memberships.dropna(subset=["urn", "groupUid"]).drop_duplicates()

    logger.info("Group memberships: %d rows", len(memberships))
    return memberships.reset_index(drop=True)
