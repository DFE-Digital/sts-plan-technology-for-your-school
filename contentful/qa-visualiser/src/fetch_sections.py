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
        logger.info("Validating retrieved sections")
        return [Section.model_validate(item) for item in data.json()]
    except RequestException as ex:
        logger.error(f"Error fetching sections: {ex}")
        raise ex
    except (ValidationError, TypeError) as ex:
        logger.error(f"Error converting response to Sections: {ex}")
        raise ex


def fetch_recommendation_chunks() -> dict[str, list[str]]:
    """
    Fetches all RecommendationChunks from the chunks API in /api/cms 
    Returns a dictionary mapping answerId to RecommendationHeader.
    """
    token = os.getenv("PLANTECH_API_KEY")
    base_url = os.getenv("PLANTECH_API_URL")
    
    total_items = []  # Store all results
    page_number = 1  # Start from the first page

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
            logger.info(f"Retrieved {len(items)} items from page {page_number}, total so far: {len(total_items)}")

            page_number += 1  

        recommendation_map = {}

        for item in total_items:
            answer_id = item.get("answerId")
            recommendation_header = item.get("recommendationHeader")  # Ensure correct field name

            if answer_id and recommendation_header:
                if answer_id not in recommendation_map:
                    recommendation_map[answer_id] = []  # Initialize as list

                recommendation_map[answer_id].append(recommendation_header)  # Store multiple recommendations

        logger.info(f"Successfully retrieved {len(recommendation_map)} unique answer mappings.")
        return recommendation_map

    except (RequestException, TypeError) as ex:
        logger.error(f"Error fetching recommendation chunks: {ex}")
        return {}
