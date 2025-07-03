import zipfile
from pathlib import Path

import pandas as pd

from src.constants import DOWNLOAD_PATH


def _extract_data(zip_path: Path) -> None:
    """Extract contents of zip file, allowing for multiple layers of zip folders"""
    with zipfile.ZipFile(zip_path, "r") as zip_file:
        zip_file.extractall(path=DOWNLOAD_PATH)
        for file in zip_file.namelist():
            if file.endswith(".zip"):
                _extract_data(zip_path.parent / file)


def _read_csv(path: Path, columns: dict[str, str]) -> pd.DataFrame:
    """Read a csv file keeping all columns as strings and rename columns as per provided columns mapping"""
    df = pd.read_csv(
        path,
        encoding="latin1",
        usecols=list(columns.keys()),
        keep_default_na=False,
        dtype=str,
    )
    df.rename(columns=columns, inplace=True)
    return df


def _parse_groups() -> pd.DataFrame:
    """Parse the groups.csv file"""
    columns = {
        "Group UID": "uid",
        "Group Name": "groupName",
        "Group Type": "groupType",
        "Group Status": "groupStatus",
    }
    return _read_csv(DOWNLOAD_PATH / "groups.csv", columns)


def _parse_links() -> pd.DataFrame:
    """Parse the links.csv file"""
    columns = {
        "Group UID": "groupUid",
        "EstablishmentName": "establishmentName",
        "URN": "urn",
    }
    return _read_csv(DOWNLOAD_PATH / "links.csv", columns)


def extract_groups_and_links() -> tuple[pd.DataFrame, pd.DataFrame]:
    """Extract the downloaded zip folder and retrieve groups and links"""
    _extract_data(DOWNLOAD_PATH / "extract.zip")
    groups = _parse_groups()
    links = _parse_links()
    return groups, links