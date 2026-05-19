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


def get_file_path(csv_stem: str, specific_date: date | None = None) -> Path:
    DOWNLOAD_PATH.mkdir(exist_ok=True)

    date_str = (
        date.today().strftime("%Y%m%d")
        if specific_date is None
        else specific_date.strftime("%Y%m%d")
    )

    return DOWNLOAD_PATH / f"{csv_stem}{date_str}.csv"


def get_latest_file_path(csv_stem: str) -> Path:
    existing_files = sorted(DOWNLOAD_PATH.glob(f"{csv_stem}*.csv"), reverse=True)
    if not existing_files:
        raise FileNotFoundError(f"No CSV matching {csv_stem}*.csv in {DOWNLOAD_PATH}")

    return existing_files[0]


def get_logger(name: str) -> Logger:
    return logging.getLogger(name)


def read_dataframe(path: Path):
    return read_csv(path, encoding=ENCODING, dtype=str, low_memory=False)
