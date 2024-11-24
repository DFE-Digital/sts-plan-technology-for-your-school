import os

import requests
from dotenv import load_dotenv

from src.models import Section

load_dotenv()


def fetch_sections() -> list[Section]:
    token = os.getenv("PLANTECH_API_KEY")
    data = requests.get(
        "https://localhost:8080/api/cms/sections",
        headers={
            "Accept": "application/json",
            "Authorization": f"Bearer {token}",
        },
    )
    sections: list[Section] = data.json()
    return sections
