import logging
import os

import requests
from dotenv import load_dotenv
from pydantic import ValidationError
from requests.exceptions import JSONDecodeError, RequestException

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
        return []
    except (JSONDecodeError, ValidationError, TypeError) as ex:
        logger.error(f"Error converting response to Sections: {ex}")
        return []
