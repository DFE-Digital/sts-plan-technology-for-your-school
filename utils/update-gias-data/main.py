import argparse
import logging
import os
import os.path

from dotenv import load_dotenv

from src.constants import CONNECTION_STRING_ENV_VAR
from src.extract_data import extract_gias_data
from src.fetch_data import fetch_and_save_gias_data
from src.update_database import update_database
from src.utils import get_logger

load_dotenv()

logger = get_logger(__name__)


def main(connection_string: str, skip_validation: bool = False):
    if skip_validation:
        logger.info(
            "Starting GIAS data update process with validation checks disabled."
        )
        logger.info(
            "GIAS data validation will be skipped - abnormal data patterns will not prevent database updates."
        )
    else:
        logger.info("Starting GIAS data update process with validation checks enabled.")

    fetch_and_save_gias_data()
    data = extract_gias_data()
    update_database(data, connection_string, skip_validation)

    logger.info("\nGIAS update complete")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Update GIAS establishment data in the database"
    )
    parser.add_argument(
        "--skip-validation",
        action="store_true",
        help="Skip GIAS data validation checks (use when expecting abnormal data patterns)",
    )
    args = parser.parse_args()

    connection_string = os.getenv(CONNECTION_STRING_ENV_VAR)
    if not connection_string:
        env_file_path = os.path.join(os.getcwd(), ".env")

        if not os.path.exists(env_file_path):
            logger.error(
                "Environment variable `%s` is not set, and the `.env` file is missing. "
                "Copy `.env.example` to `.env` and fill in the connection string.",
                CONNECTION_STRING_ENV_VAR,
            )
            exit(101)
        else:
            logger.error(
                "Environment variable `%s` is not set. Please ensure it is defined in "
                "the `.env` file or set via OS environment variables.",
                CONNECTION_STRING_ENV_VAR,
            )
            exit(100)

    main(connection_string=connection_string, skip_validation=args.skip_validation)
