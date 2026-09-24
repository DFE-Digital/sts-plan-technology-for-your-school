from pandas import DataFrame

from dataclasses import dataclass


@dataclass
class GiasData:
    # Lookups
    ## Establishment lookups
    administrative_districts: DataFrame
    administrative_wards: DataFrame
    admissions_policies: DataFrame
    establishment_statuses: DataFrame
    establishment_type_groups: DataFrame
    genders: DataFrame
    government_office_regions: DataFrame
    local_authorities: DataFrame
    parliamentary_constituencies: DataFrame
    phases: DataFrame
    religious_characters: DataFrame
    sixth_form_statuses: DataFrame
    trusts: DataFrame
    trust_school_flags: DataFrame
    types_of_establishment: DataFrame
    urban_rural_classifications: DataFrame

    ## Group lookups
    group_statuses: DataFrame
    group_types: DataFrame

    # Core entities
    establishments: DataFrame
    establishment_groups: DataFrame

    # Child tables
    group_membership: DataFrame
    links: DataFrame
