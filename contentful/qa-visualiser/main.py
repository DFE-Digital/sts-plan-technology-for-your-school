import logging
import os

from src.fetch_sections import (
    fetch_recommendation_chunks,
    fetch_sections,
)
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

    # set up empty structures
    completing_map: dict[str, list[str]] = {}
    inprogress_map: dict[str, list[str]] = {}
    all_recommendations: set[str] = set()

    if display_recommendations:
        completing_map, inprogress_map, all_recommendations = fetch_recommendation_chunks()

    sections = fetch_sections()
    process_sections(
        sections=sections,
        completing_map=completing_map,
        inprogress_map=inprogress_map,
        all_recommendations=all_recommendations,
    )


if __name__ == "__main__":
    main()
