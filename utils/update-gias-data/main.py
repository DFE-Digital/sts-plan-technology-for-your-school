import logging

from dotenv import load_dotenv

from src.extract_data import extract_groups_and_links
from src.fetch_data import fetch_and_save_gias_data
from src.update_database import update_database

load_dotenv()
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(message)s",
)


def main():
    fetch_and_save_gias_data()
    groups, links = extract_groups_and_links()
    update_database(groups, links)


if __name__ == "__main__":
    main()
