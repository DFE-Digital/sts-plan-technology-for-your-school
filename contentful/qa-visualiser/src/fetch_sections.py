import logging
import os

import requests
from dotenv import load_dotenv
from pydantic import ValidationError
from requests.exceptions import RequestException

from src.models import Section

load_dotenv()
logger = logging.getLogger(__name__)


def fetch_sections() -> list[Section]:
    """
    Calls to one of the plan tech cms endpoints to retrieve sections
    and serializes to a list of Section models
    """
    token = os.getenv("PLANTECH_API_KEY")
    base_url = os.getenv("PLANTECH_API_URL")
    try:
        logger.info(f"Fetching sections from {base_url}")
        data = requests.get(
            f"{base_url}/sections",
            headers={
                "Accept": "application/json",
                "Authorization": f"Bearer {token}",
            },
        )
        data.raise_for_status()

        result = []
        payload = data.json()
        if not isinstance(payload, list):
            raise TypeError(f"Unexpected /sections payload type: {type(payload)}")

        logger.info("Validating retrieved sections")
        for i, item in enumerate(payload):
            name = item.get("name", "<unnamed>")
            logger.info(f"Validating section '{name}' (index [{i}])")
            result.append(Section.model_validate(item))

        return result

    except RequestException as ex:
        logger.error(f"Error fetching sections: {ex}")
        raise
    except (ValidationError, TypeError) as ex:
        logger.error(f"Error converting response to Sections: {ex}")
        raise


def fetch_recommendation_chunks() -> tuple[dict[str, list[str]], dict[str, list[str]], set[str]]:
    """
    Fetches paginated RecommendationChunks from {PLANTECH_API_URL}/chunks/{page}
    Returns:
      - completing_map: answer_id -> list of recommendation headers the answer sets status completed
      - inprogress_map: answer_id -> list of recommendation headers the answer sets ststus in-progress
      - all_recommendations: set of all recommendation headers (to render all rec nodes)
    """
    token = os.getenv("PLANTECH_API_KEY")
    base_url = os.getenv("PLANTECH_API_URL")

    total_items = [] # Store all results
    page_number = 1 # Start from the first page

    try:
        logger.info(f"Fetching recommendation chunks from {base_url}/chunks/1")

        while True:
            response = requests.get(
                f"{base_url}/chunks/{page_number}",
                headers={
                    "Accept": "application/json",
                    "Authorization": f"Bearer {token}",
                },
            )
            response.raise_for_status()
            data = response.json()

            items = data.get("items", [])

            if not items:
                logger.info(f"No more items on page {page_number}. Stopping pagination.")
                break

            total_items.extend(items)
            logger.info(
                f"Retrieved {len(items)} items from page {page_number}, total so far: {len(total_items)}"
            )
            page_number += 1

        completing_map: dict[str, list[str]] = {}
        inprogress_map: dict[str, list[str]] = {}
        all_recommendations: set[str] = set()

        for item in total_items:
            completing_id = item.get("CompletingAnswerId") or item.get("completingAnswerId")
            inprogress_id = item.get("InProgressAnswerId") or item.get("inProgressAnswerId")
            header = item.get("RecommendationHeader") or item.get("recommendationHeader")

            if not header:
                logger.warning(f"Skipping chunk without RecommendationHeader: {item}")
                continue

            all_recommendations.add(header)

            if completing_id:
                completing_map.setdefault(completing_id, []).append(header)

            if inprogress_id:
                inprogress_map.setdefault(inprogress_id, []).append(header)

        logger.info(
            "Built maps: %d completing keys, %d in-progress keys, %d total recommendations",
            len(completing_map),
            len(inprogress_map),
            len(all_recommendations),
        )
        return completing_map, inprogress_map, all_recommendations

    except (RequestException, TypeError) as ex:
        logger.error(f"Error fetching recommendation chunks: {ex}")
        return {}, {}, set()
