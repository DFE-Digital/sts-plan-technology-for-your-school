import os
import yaml
from pathlib import Path
from notifications_python_client.notifications import NotificationsAPIClient

API_KEY = os.environ.get("NOTIFICATIONS_API_KEY")

TEMPLATE_ID = "0a0921f4-db1f-4a9c-9791-af14411f8b54"

recipients = [
    "drew.morgan@education.gov.uk",
    "gilaine.young@education.gov.uk",
    "jag.nahl@education.gov.uk",
    "laura.steele@education.gov.uk"
    "rian.thwaite@education.gov.uk",
    "sofia.costa@education.gov.uk",
    "technology.planning@education.gov.uk"
]

def load_all_failures(directory="downloaded-failures"):
    failures = []
    failures_dir = Path(directory)

    if not failures_dir.exists():
        return failures

    for failure_file in failures_dir.glob("*.yml"):
        with open(failure_file) as f:
            data = yaml.safe_load(f)
            if data and "failures" in data:
                failures.extend(data["failures"])

    return failures

failures = load_all_failures()

if not failures:
    print("✅ No failed environments, skipping notification.")
    exit(0)

env_summary = "\n".join(
    f"- {env['name']} ({env['url']}) responded with HTTP {env['status']}" for env in failures
)

personalisation = {
    "environment summary": env_summary
}

client = NotificationsAPIClient(API_KEY)

print(f"✉️  Sending downtime alert emails")
for recipient in recipients:
    client.send_email_notification(
        email_address=recipient.strip(),
        template_id=TEMPLATE_ID,
        personalisation=personalisation,
        reference="Environment Down Alert"
    )
