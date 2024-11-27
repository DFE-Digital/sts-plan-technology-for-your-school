import logging

from src.fetch_sections import fetch_sections
from src.generate_visualisations import process_sections

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(message)s",
)


def main():
    sections = fetch_sections()
    process_sections(sections)


if __name__ == "__main__":
    main()
