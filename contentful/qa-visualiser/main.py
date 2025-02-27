import logging
import os

from src.fetch_sections import fetch_recommendation_chunks, fetch_sections
from src.generate_visualisations import process_sections

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(message)s",
)


def main():
    display_recommendations = os.getenv("DISPLAY_RECOMMENDATIONS", "false").lower() in [
        "true",
        "1",
    ]
    recommendation_map = []

    if display_recommendations:
        recommendation_map = fetch_recommendation_chunks()

    sections = fetch_sections()
    process_sections(sections, recommendation_map)


if __name__ == "__main__":
    main()
