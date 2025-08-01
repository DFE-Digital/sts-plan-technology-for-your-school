import argparse
import logging
import os
import os.path

from src.constants import CONNECTION_STRING_ENV_VAR
from src.extract_data import extract_groups_and_links_from_csv
from src.fetch_data import fetch_and_save_gias_data
from src.update_database import update_database

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(message)s",
)

logger = logging.getLogger(__name__)

def main(connection_string: str, skip_gias_validation: bool = False):
    if skip_gias_validation:
        logger.info("Starting GIAS data update process with validation checks disabled.")
        logger.info("GIAS data validation will be skipped - abnormal data patterns will not prevent database updates.")
    else:
        logger.info("Starting GIAS data update process with validation checks enabled.")

    fetch_and_save_gias_data()
    groups, links = extract_groups_and_links_from_csv()
    update_database(groups, links, connection_string, skip_gias_validation)

    logger.info("GIAS data update process completed.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Update GIAS establishment data in the database")
    parser.add_argument("--skip-validation", action="store_true", help="Skip GIAS data validation checks (use when expecting abnormal data patterns)")
    args = parser.parse_args()

    connection_string = os.getenv(CONNECTION_STRING_ENV_VAR)
    if not connection_string:
        env_file_path = os.path.join(os.getcwd(), ".env")
        if not os.path.exists(env_file_path):
            logger.error(f"Environment variable `{CONNECTION_STRING_ENV_VAR}` is not set, and the `.env` file is missing.")
            logger.error("Please create a `.env` file in the project root. You can use `.env.template` as a starting point.")
            exit(101)
        else:
            logger.error(f"Environment variable `{CONNECTION_STRING_ENV_VAR}` is not set. Please ensure it is defined in the `.env` file (or set via OS environment variables).")
            exit(100)

    main(connection_string=connection_string, skip_gias_validation=args.skip_validation)
