import logging
import os

import requests
from dotenv import load_dotenv
from pydantic import ValidationError
from requests.exceptions import JSONDecodeError, RequestException

from src.models import Section

load_dotenv()
logger = logging.getLogger(__name__)

TOKEN = os.getenv("PLANTECH_API_KEY")
BASE_URL = os.getenv("PLANTECH_API_URL")

def fetch_sections() -> list[Section]:
    try:
        logger.info(f"Fetching sections from {BASE_URL}")
        data = requests.get(
            f"{BASE_URL}/sections",
            headers={
                "Accept": "application/json",
                "Authorization": f"Bearer {TOKEN}",
            },
            verify=False,
        )
        logger.info("Validating retrieved sections")
        return [Section.model_validate(item) for item in data.json()]
    except RequestException as ex:
        logger.error(f"Error fetching sections: {ex}")
        return []
    except (JSONDecodeError, ValidationError, TypeError) as ex:
        logger.error(f"Error converting response to Sections: {ex}")
        return []
