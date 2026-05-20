from pathlib import Path

CONNECTION_STRING_ENV_VAR = "CONNECTION_STRING_PLANTECH_GIAS_UPDATE"
CSV_STEMS = [
    "edubasealldata",
    "allgroupsdata",
    "alllinksdata",
]
DOWNLOAD_PATH = Path(__file__).parent.parent / "downloads"
EDUBASE_DOWNLOADS_BASE_URL = (
    "https://ea-edubase-api-prod.azurewebsites.net/edubase/downloads/public"
)
ENCODING = "latin-1"
