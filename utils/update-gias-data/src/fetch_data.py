import requests

from datetime import date
from pandas import Timedelta
from pathlib import Path
from tqdm import tqdm

from src.constants import EDUBASE_DOWNLOADS_BASE_URL, CSV_STEMS
from src.utils import get_file_path, get_logger

logger = get_logger(__name__)

CHUNK_SIZE = 65_536
DEFAULT_TIMEOUT_SECONDS = 60


def _download(url: str, dest: Path, is_test_download: bool = False) -> None:
    logger.info("Downloading %s", url)
    with requests.get(url, stream=True, timeout=DEFAULT_TIMEOUT_SECONDS) as request:
        request.raise_for_status()

        if is_test_download:
            logger.info("Test download successful, skipping saving")
            return

        total = int(request.headers.get("content-length", 0))
        with (
            dest.open("wb") as f,
            tqdm(total=total, unit="B", unit_scale=True, desc=dest.name) as bar,
        ):
            for chunk in request.iter_content(CHUNK_SIZE):
                f.write(chunk)
                bar.update(len(chunk))

    logger.info("Saved %s", dest)


def fetch_and_save_gias_data() -> None:
    """Download the four GIAS CSV files for today's date into DATA_PATH."""

    file_date = date.today()
    files_available = True

    for stem in CSV_STEMS:
        file_path = get_file_path(stem, file_date)
        url = f"{EDUBASE_DOWNLOADS_BASE_URL}/{file_path.name}"
        try:
            _download(url, file_path, is_test_download=True)
        except requests.HTTPError as e:
            if e.response.status_code == 404:
                logger.warning(
                    "No data available for %s, will use yesterday's date", file_date
                )
                file_date -= Timedelta(days=1)
                break
            else:
                raise

    for stem in CSV_STEMS:
        file_path = get_file_path(stem, file_date)
        if file_path.exists():
            logger.info("%s already exists, skipping download", file_path.name)
            continue

        url = f"{EDUBASE_DOWNLOADS_BASE_URL}/{file_path.name}"
        _download(url, file_path)
