import logging
import re

from datetime import date
from logging import Logger
from pandas import read_csv
from pathlib import Path

from src.constants import DOWNLOAD_PATH, ENCODING

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(message)s",
)


def get_file_date(file_path: Path) -> date:
    DOWNLOAD_PATH.mkdir(exist_ok=True)
    matches = re.search(r"\d{8}", file_path.stem)
    if not matches:
        raise ValueError(f"No date found in filename {file_path.name}")
    file_date = matches.group()
    return date(int(file_date[:4]), int(file_date[4:6]), int(file_date[6:8]))


def get_file_path(csv_stem: str, specific_date: date | None = None) -> Path:
    DOWNLOAD_PATH.mkdir(exist_ok=True)

    date_str = (
        date.today().strftime("%Y%m%d")
        if specific_date is None
        else specific_date.strftime("%Y%m%d")
    )

    return DOWNLOAD_PATH / f"{csv_stem}{date_str}.csv"


def get_latest_file_path(csv_stem: str) -> Path:
    matches = sorted(DOWNLOAD_PATH.glob(f"{csv_stem}*.csv"))
    if not matches:
        raise FileNotFoundError(f"No CSV matching {csv_stem}*.csv in {DOWNLOAD_PATH}")

    return DOWNLOAD_PATH / matches[0].name


def get_logger(name: str) -> Logger:
    return logging.getLogger(name)


def read_dataframe(path: Path):
    return read_csv(path, encoding=ENCODING, dtype=str, low_memory=False)
