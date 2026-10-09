import pandas as pd

from src.classes import GiasData
from src.utils import get_latest_file_path, get_logger, read_dataframe

logger = get_logger(__name__)


def _coerce_int(series: pd.Series) -> pd.Series:
    stripped = series.astype(str).str.strip()
    result = pd.to_numeric(stripped, errors="raise").astype("Int64")

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

    if not set([input_code_col, input_name_col]).issubset(df.columns):
        raise ValueError(
            "Required columns %s must exist in the dataframe",
            [input_code_col, input_name_col],
        )

    out = df[cols].copy()
    null_mask = out[list([input_name_col])].isna().any(axis=1)
    unique_codes = out.loc[null_mask, input_code_col].unique()

    if len(unique_codes) > 1:
        raise ValueError(
            f"NA values found in name column {input_name_col} "
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
        out.dropna(subset=[output_code_col])
        .dropna(subset=[output_name_col])
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
    map = read_dataframe(get_latest_file_path("links_edubasealldata"))

    logger.info(
        "Loaded CSVs - alldata:%d allgroups:%d alllinks:%d links:%d\n",
        len(edu),
        len(grp),
        len(lnk),
        len(map),
    )

    logger.info("Extracting data")
    establishments = _establishments(edu)
    establishment_groups = _establishment_groups(grp)
    group_membership = _group_membership(lnk)
    links = _links(map)

    administrative_districts = _administrative_districts(edu)
    administrative_wards = _administrative_wards(edu)
    admissions_policies = _admissions_policies(edu)
    establishment_statuses = _establishment_statuses(edu)
    establishment_type_groups = _establishment_type_groups(edu)
    genders = _genders(edu)
    government_office_regions = _government_office_regions(edu)
    group_statuses = _group_statuses(grp)
    group_types = _group_types(grp)
    local_authorities = _local_authorities(edu)
    parliamentary_constituencies = _parliamentary_constituencies(edu)
    phases = _phases(edu)
    religious_characters = _religious_characters(edu)
    sixth_form_statuses = _sixth_form_statuses(edu)
    trusts = _trusts(edu)
    trust_school_flags = _trust_school_flags(edu)
    types_of_establishment = _types_of_establishment(edu)
    urban_rural_classifications = _urban_rural_classifications(edu)

    logger.info("Extracted data - establishments: %d", len(establishments))
    logger.info("Extracted data - establishmentGroups: %d", len(establishment_groups))
    logger.info("Extracted data - groupMembership: %d", len(group_membership))
    logger.info("Extracted data - links: %d", len(links))

    logger.info(
        "Extracted data - administrativeDistricts: %d", len(administrative_districts)
    )
    logger.info("Extracted data - administrativeWards: %d", len(administrative_wards))
    logger.info("Extracted data - admissionsPolicies: %d", len(admissions_policies))
    logger.info(
        "Extracted data - establishmentStatuses: %d", len(establishment_statuses)
    )
    logger.info(
        "Extracted data - establishmentTypeGroups: %d", len(establishment_type_groups)
    )
    logger.info("Extracted data - genders: %d", len(genders))
    logger.info(
        "Extracted data - governmentOfficeRegions: %d", len(government_office_regions)
    )
    logger.info("Extracted data - groupStatuses: %d", len(group_statuses))
    logger.info("Extracted data - groupTypes: %d", len(group_types))
    logger.info("Extracted data - localAuthorities: %d", len(local_authorities))
    logger.info(
        "Extracted data - parliamentaryConstituencies: %d",
        len(parliamentary_constituencies),
    )
    logger.info("Extracted data - phases: %d", len(phases))
    logger.info("Extracted data - religiousCharacters: %d", len(religious_characters))
    logger.info("Extracted data - sixthFormStatuses: %d", len(sixth_form_statuses))
    logger.info("Extracted data - trusts: %d", len(trusts))
    logger.info("Extracted data - trustSchoolFlags: %d", len(trust_school_flags))
    logger.info(
        "Extracted data - typesOfEstablishment: %d", len(types_of_establishment)
    )
    logger.info(
        "Extracted data - urbanRuralClasses: %d", len(urban_rural_classifications)
    )

    establishment_urns = set(establishments["urn"].dropna())
    establishment_group_uids = set(establishment_groups["groupUid"].dropna())

    orphaned_urn = group_membership["urn"].notna() & ~group_membership["urn"].isin(
        establishment_urns
    )

    warnings = 0
    if orphaned_urn.any():
        logger.info(
            "Found %d URNs not present in establishment from groupMembership.",
            int(orphaned_urn.sum()),
        )
        # for orphaned in group_membership.loc[orphaned_urn, "urn"].unique():
        #     logger.warning(
        #         "    URN %s",
        #         orphaned,
        #     )

        logger.warning(
            "Dropping %d URNs not present in establishments from groupMembership (usually children's centres).",
            int(orphaned_urn.sum()),
        )
        warnings += 1

    orphaned_uid = group_membership["groupUid"].notna() & ~group_membership[
        "groupUid"
    ].isin(establishment_group_uids)

    if orphaned_uid.any():
        logger.info(
            "Found %d Group UIDs not present in establishmentGroups from groupMembership.",
            int(orphaned_uid.sum()),
        )
        # for orphaned in group_membership.loc[orphaned_uid, "groupUid"].unique():
        #     logger.warning(
        #         "    Group UID %s",
        #         orphaned,
        #     )

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

    # Remove linkedUrns in links that aren't in establishments, as these would fail FK constraints
    # and aren't useful without a corresponding establishment record.
    # Log how many are dropped but don't raise an error.
    orphaned_linked_urn = links["linkedUrn"].notna() & ~links["linkedUrn"].isin(
        establishment_urns
    )
    if orphaned_linked_urn.any():
        logger.info(
            "Found %d linked URNs not present in establishmentGroups from groupMembership.",
            int(orphaned_linked_urn.sum()),
        )
        # for orphaned in links.loc[orphaned_linked_urn, "linkedUrn"].unique():
        #     logger.warning(
        #         "    Linked URN %s",
        #         orphaned,
        #     )

        logger.warning(
            "Dropping %d linked URNs not present in establishments from links (usually future-dated links to establishments that do not yet exist).",
            int(orphaned_linked_urn.sum()),
        )
        links = links[~orphaned_linked_urn].reset_index(drop=True)

    return GiasData(
        # Core
        establishments=establishments,
        establishment_groups=establishment_groups,
        group_membership=group_membership,
        links=links,
        # Establishment lookups
        administrative_districts=administrative_districts,
        administrative_wards=administrative_wards,
        admissions_policies=admissions_policies,
        establishment_statuses=establishment_statuses,
        establishment_type_groups=establishment_type_groups,
        genders=genders,
        government_office_regions=government_office_regions,
        local_authorities=local_authorities,
        parliamentary_constituencies=parliamentary_constituencies,
        phases=phases,
        religious_characters=religious_characters,
        sixth_form_statuses=sixth_form_statuses,
        trusts=trusts,
        trust_school_flags=trust_school_flags,
        types_of_establishment=types_of_establishment,
        urban_rural_classifications=urban_rural_classifications,
        # Group lookups
        group_statuses=group_statuses,
        group_types=group_types,
    )


# ---------------------------------------------------------------------------
# Lookup tables - establishments
# ---------------------------------------------------------------------------


def _administrative_districts(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "DistrictAdministrative (code)",
        "administrativeDistrictCode",
        False,
        "DistrictAdministrative (name)",
        "administrativeDistrictName",
        blank_replacement="Unknown",
    )


def _administrative_wards(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "AdministrativeWard (code)",
        "administrativeWardCode",
        False,
        "AdministrativeWard (name)",
        "administrativeWardName",
        blank_replacement="Unknown",
    )


def _admissions_policies(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "AdmissionsPolicy (code)",
        "admissionsPolicyCode",
        True,
        "AdmissionsPolicy (name)",
        "admissionsPolicyName",
        blank_replacement="Unknown",
    )


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


def _establishment_type_groups(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "EstablishmentTypeGroup (code)",
        "establishmentTypeGroupCode",
        True,
        "EstablishmentTypeGroup (name)",
        "establishmentTypeGroupName",
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


def _government_office_regions(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "GOR (code)",
        "governmentOfficeRegionCode",
        False,
        "GOR (name)",
        "governmentOfficeRegionName",
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


def _parliamentary_constituencies(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "ParliamentaryConstituency (code)",
        "parliamentaryConstituencyCode",
        False,
        "ParliamentaryConstituency (name)",
        "parliamentaryConstituencyName",
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


def _religious_characters(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "ReligiousCharacter (code)",
        "religiousCharacterCode",
        True,
        "ReligiousCharacter (name)",
        "religiousCharacterName",
        blank_replacement="Unknown",
    )


def _sixth_form_statuses(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "OfficialSixthForm (code)",
        "sixthFormStatusCode",
        True,
        "OfficialSixthForm (name)",
        "sixthFormStatusName",
        blank_replacement="Unknown",
    )


def _trusts(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "Trusts (code)",
        "trustCode",
        True,
        "Trusts (name)",
        "trustName",
        blank_replacement="Unknown",
    )


def _trust_school_flags(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "TrustSchoolFlag (code)",
        "trustSchoolFlagCode",
        True,
        "TrustSchoolFlag (name)",
        "trustSchoolFlagName",
        blank_replacement="Unknown",
    )


def _types_of_establishment(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "TypeOfEstablishment (code)",
        "typeOfEstablishmentCode",
        True,
        "TypeOfEstablishment (name)",
        "typeOfEstablishmentName",
        blank_replacement="Unknown",
    )


def _urban_rural_classifications(edu: pd.DataFrame) -> pd.DataFrame:
    return _lookup(
        edu,
        "UrbanRural (code)",
        "urbanRuralCode",
        False,
        "UrbanRural (name)",
        "urbanRuralName",
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
        # Identifiers
        "URN": "urn",
        "UPRN": "uprn",
        "UKPRN": "ukprn",
        "EstablishmentNumber": "establishmentNumber",
        "EstablishmentName": "establishmentName",
        # Lookups
        "AdministrativeWard (code)": "administrativeWardCode",
        "AdmissionsPolicy (code)": "admissionsPolicyCode",
        "DistrictAdministrative (code)": "administrativeDistrictCode",
        "EstablishmentStatus (code)": "establishmentStatusCode",
        "EstablishmentTypeGroup (code)": "establishmentTypeGroupCode",
        "Gender (code)": "genderCode",
        "GOR (code)": "governmentOfficeRegionCode",
        "LA (code)": "localAuthorityCode",
        "ParliamentaryConstituency (code)": "parliamentaryConstituencyCode",
        "PhaseOfEducation (code)": "phaseCode",
        "ReligiousCharacter (code)": "religiousCharacterCode",
        "OfficialSixthForm (code)": "sixthFormStatusCode",
        "Trusts (code)": "trustCode",
        "TrustSchoolFlag (code)": "trustSchoolFlagCode",
        "TypeOfEstablishment (code)": "typeOfEstablishmentCode",
        "UrbanRural (code)": "urbanRuralCode",
    }
    out = edu[list(cols)].rename(columns=cols).copy()

    int_cols = [
        "urn",
        "uprn",
        "ukprn",
        "establishmentNumber",
        "admissionsPolicyCode",
        "establishmentStatusCode",
        "establishmentTypeGroupCode",
        "genderCode",
        "localAuthorityCode",
        "phaseCode",
        "religiousCharacterCode",
        "sixthFormStatusCode",
        "trustCode",
        "trustSchoolFlagCode",
        "typeOfEstablishmentCode",
    ]

    for col in int_cols:
        out[col] = _coerce_int(out[col])

    # Count rows with a blank establishment name and warn if any are found
    blank_name_count = (out["establishmentName"].str.strip() == "").sum()
    if blank_name_count > 0:
        raise ValueError(
            f"Found {blank_name_count} rows with blank establishment name. Cannot proceed."
        )

    return out.dropna(subset=["urn"]).reset_index(drop=True)


def _establishment_groups(grp: pd.DataFrame) -> pd.DataFrame:
    cols = {
        # Identifiers
        "Group UID": "groupUid",
        "Group ID": "groupId",
        "UKPRN": "ukprn",
        "Group Name": "groupName",
        # Lookups
        "Group Status (code)": "groupStatusCode",
        "Group Type (code)": "groupTypeCode",
    }
    out = grp[list(cols)].rename(columns=cols).copy()

    int_cols = ["groupUid", "ukprn", "groupTypeCode"]
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


def _links(map: pd.DataFrame) -> pd.DataFrame:
    cols = {
        "URN": "urn",
        "LinkURN": "linkedUrn",
        "LinkType": "linkType",
        "LinkEstablishedDate": "dateLinked",
    }
    out = map[list(cols)].rename(columns=cols).copy()

    for col in (
        "urn",
        "linkedUrn",
    ):
        out[col] = _coerce_int(out[col])

    out["dateLinked"] = pd.to_datetime(
        out["dateLinked"], format="%d-%m-%Y", errors="coerce"
    ).dt.date

    # Blank → None for non-date dateLinked column
    out["dateLinked"] = out["dateLinked"].where(out["dateLinked"].notnull(), None)

    return out.dropna(subset=["urn"]).reset_index(drop=True)
