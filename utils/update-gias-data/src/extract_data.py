import pandas as pd

from src.classes import GiasData
from src.utils import get_latest_file_path, get_logger, read_dataframe

logger = get_logger(__name__)


def _coerce_int(series: pd.Series) -> pd.Series:
    stripped = series.astype(str).str.strip()
    result = pd.to_numeric(stripped, errors="coerce").astype("Int64")

    # Preserve the original index to avoid alignment issues
    result.index = series.index
    return result


def _lookup(
    df: pd.DataFrame,
    input_code_col: str,
    output_code_col: str,
    coerce_code_to_int: bool,
    input_name_col: str,
    output_name_col: str,
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
    cols = [input_code_col, input_name_col]

    if not set([input_code_col, input_name_col]).issubset(cols):
        raise ValueError(
            "Required columns %s must exist in the dataframe",
            [input_code_col, input_name_col],
        )

    out = df[cols].copy()
    null_mask = out[list([input_name_col])].isna().any(axis=1)
    unique_codes = out.loc[null_mask, input_code_col].unique()

    if len(unique_codes) > 1:
        raise ValueError(
            f"NA values found in name columns {input_name_col} "
            f"with multiple different codes: {sorted(unique_codes)}. Cannot proceed."
        )

    if len(unique_codes) == 1:
        logger.info(
            f"NA values found in column {input_name_col}, "
            f"but all have the same code {unique_codes[0]}. "
            f"Name value will be replaced with '{blank_replacement}'."
        )

        out[input_name_col] = out[input_name_col].fillna(blank_replacement)

    # Get rows where required columns contain NA values, if any
    number_of_nulls = out[input_name_col].isna().sum()
    if number_of_nulls > 0:
        raise ValueError(
            f"{number_of_nulls} rows in {input_name_col} have NA names "
            f"that fillna('{blank_replacement}') didn't resolve. "
            f"First 5: {out.loc[out[input_name_col].isna()].head(5).to_dict('records')}"
        )

    if coerce_code_to_int:
        out[input_code_col] = _coerce_int(out[input_code_col])

    out.columns = [output_code_col, output_name_col]

    return (
        out.dropna(subset=[output_name_col])
        .drop_duplicates(subset=[output_code_col])
        .reset_index(drop=True)
    )


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
    return _lookup(
        edu,
        "EstablishmentStatus (code)",
        "establishmentStatusCode",
        True,
        "EstablishmentStatus (name)",
        "establishmentStatusName",
        blank_replacement="Unknown",
    )


def _genders(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "Gender (code)",
        "genderCode",
        True,
        "Gender (name)",
        "genderName",
        blank_replacement="Unknown",
    )


def _local_authorities(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "LA (code)",
        "localAuthorityCode",
        True,
        "LA (name)",
        "localAuthorityName",
        blank_replacement="Unknown",
    )


def _phases(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "PhaseOfEducation (code)",
        "phaseCode",
        True,
        "PhaseOfEducation (name)",
        "phaseName",
        blank_replacement="Unknown",
    )


# ---------------------------------------------------------------------------
# Lookup tables - groups
# ---------------------------------------------------------------------------


def _group_statuses(grp: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        grp,
        "Group Status (code)",
        "groupStatusCode",
        False,
        "Group Status",
        "groupStatusName",
        blank_replacement="Unknown",
    )


def _group_types(grp: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        grp,
        "Group Type (code)",
        "groupTypeCode",
        True,
        "Group Type",
        "groupTypeName",
        blank_replacement="Unknown",
    )


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
            f"Found {blank_name_count} rows with blank establishment name. Cannot proceed."
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
