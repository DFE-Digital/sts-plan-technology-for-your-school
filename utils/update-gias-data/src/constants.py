from pathlib import Path

API_URL = "https://ea-edubase-api-prod.azurewebsites.net/edubase/downloads/public"
CONNECTION_STRING_ENV_VAR = "CONNECTION_STRING_PLANTECH_GIAS_UPDATE"
CSV_STEMS = ["edubasealldata", "allgroupsdata", "alllinksdata", "links_edubasealldata"]
DOWNLOAD_PATH = Path(__file__).parent.parent / "downloads"
ENCODING = "latin-1"
