import requests

from datetime import date, timedelta
from pandas import Timedelta
from pathlib import Path
from tqdm import tqdm

from src.constants import API_URL, CSV_STEMS
from src.utils import get_file_path, get_logger

logger = get_logger(__name__)

# General
CHUNK_SIZE = 65_536
DEFAULT_MAX_DAYS_BACK = 7
DEFAULT_TIMEOUT_SECONDS = 60

# _check_data_available return values
DATA_AVAILABLE = 0
DATA_MAY_BE_UNDER_PREVIOUS_DATE = 1
EDUBASE_ERROR = 2


def _is_remote_data_available(file_date: date) -> int:
    remote_file_statuses = []

    for stem in CSV_STEMS:
        file_path = get_file_path(stem, file_date)
        url = f"{API_URL}/{file_path.name}"
        logger.info("Checking availability of %s", url)

        response = requests.get(url, stream=True, timeout=DEFAULT_TIMEOUT_SECONDS)
        remote_file_statuses.append(response.status_code)

    if all(status == 200 for status in remote_file_statuses):
        return DATA_AVAILABLE

    files_unavailable = sum(status != 200 for status in remote_file_statuses)
    if files_unavailable > 0 and files_unavailable < len(CSV_STEMS):
        logger.info(
            "%s of %s files are unavailable for %s. ",
            files_unavailable,
            len(CSV_STEMS),
            file_date,
        )
        return DATA_MAY_BE_UNDER_PREVIOUS_DATE

    if all(status == 404 for status in remote_file_statuses):
        logger.info(
            "No files found for %s - they may not have been published yet",
            file_date,
        )
        return DATA_MAY_BE_UNDER_PREVIOUS_DATE

    if any(status >= 500 for status in remote_file_statuses):
        logger.error(
            "Received server error when checking for GIAS files for %s. "
            "- this may indicate an issue with the Edubase service",
            file_date,
        )
        return EDUBASE_ERROR

    logger.error(
        "Received unexpected status codes [%s] when checking for GIAS files for %s",
        ", ".join(
            f"{file}: {status}" for file, status in zip(CSV_STEMS, remote_file_statuses)
        ),
        file_date,
    )
    return EDUBASE_ERROR


def _download(url: str, dest: Path) -> None:
    logger.info("Downloading %s", url)

    with requests.get(url, stream=True, timeout=DEFAULT_TIMEOUT_SECONDS) as request:
        request.raise_for_status()

        total = int(request.headers.get("content-length", 0))
        with (
            dest.open("wb") as f,
            tqdm(total=total, unit="B", unit_scale=True, desc=dest.name) as bar,
        ):
            for chunk in request.iter_content(CHUNK_SIZE):
                f.write(chunk)
                bar.update(len(chunk))

    logger.info("Saved %s", dest)


def _find_latest_available_gias_file_date(
    today: date, max_days_back: int = DEFAULT_MAX_DAYS_BACK
) -> date:
    remote_statuses = {}

    for days_back in range(max_days_back + 1):
        candidate_date = today - timedelta(days=days_back)

        remote_status = _is_remote_data_available(candidate_date)
        if remote_status == DATA_AVAILABLE:
            return candidate_date

        remote_statuses[candidate_date] = remote_status

    if all(
        status == DATA_MAY_BE_UNDER_PREVIOUS_DATE for status in remote_statuses.values()
    ):
        logger.warning(
            "Could not find a complete GIAS file set from the last {max_days_back} days",
            max_days_back,
        )

    logger.error(
        "Received Edubase service errors for all candidate dates in the last %s days: %s",
        max_days_back,
        ", ".join(f"  \n{date}: {status}" for date, status in remote_statuses.items()),
    )
    raise RuntimeError(
        f"Edubase service appears to be unavailable - received errors for all candidate dates in the last {max_days_back} days"
    )


def _find_latest_downloaded_gias_file_date(
    today: date, max_days_back: int = DEFAULT_MAX_DAYS_BACK
) -> date:
    for days_back in range(max_days_back + 1):
        candidate_date = today - timedelta(days=days_back)

        if all(get_file_path(stem, candidate_date).exists() for stem in CSV_STEMS):
            return candidate_date

    raise RuntimeError(
        f"Could not find a complete GIAS file set from the last {max_days_back} days"
    )


def fetch_and_save_gias_data() -> None:
    """Download the four GIAS CSV files for today's date into DATA_PATH."""

    most_recent_download_date = _find_latest_downloaded_gias_file_date(date.today())
    if most_recent_download_date == date.today():
        logger.info("Files already exist for today, skipping download")
        return

    logger.info(
        "Most recent downloaded GIAS files are from %s - checking if today's files are available",
        most_recent_download_date,
    )

    most_recent_remote_date = _find_latest_available_gias_file_date(date.today())
    if most_recent_remote_date <= most_recent_download_date:
        logger.info(
            "No more recent GIAS files available (most recent: %s), skipping download",
            most_recent_remote_date,
        )
        return

    logger.info(
        "Found more recent GIAS files available from %s",
        most_recent_remote_date,
    )

    for stem in CSV_STEMS:
        file_path = get_file_path(stem, most_recent_remote_date)
        if file_path.exists():
            logger.info("%s already exists, skipping download", file_path.name)
            continue

        url = f"{API_URL}/{file_path.name}"
        _download(url, file_path)
