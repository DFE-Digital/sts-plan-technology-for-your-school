from pandas import DataFrame

from dataclasses import dataclass


@dataclass
class GiasData:
    # Lookups
    establishment_statuses: DataFrame
    group_statuses: DataFrame
    group_types: DataFrame
    genders: DataFrame
    local_authorities: DataFrame
    phases: DataFrame

    # Core entities
    establishments: DataFrame
    establishment_groups: DataFrame

    # Child tables
    group_membership: DataFrame
