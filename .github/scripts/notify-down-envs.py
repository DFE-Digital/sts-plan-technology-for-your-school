import os
import sys
import yaml
from pathlib import Path
from notifications_python_client.notifications import NotificationsAPIClient

NOTIFICATIONS_API_KEY = os.environ.get("NOTIFICATIONS_API_KEY")
RECIPIENT_ADDRESSES = os.environ.get("RECIPIENT_ADDRESSES")

TEMPLATE_ID = "0a0921f4-db1f-4a9c-9791-af14411f8b54"

def send_email(recipient_addresses, env_summary: str) -> None:
    if not NOTIFICATIONS_API_KEY:
        raise RuntimeError("NOTIFICATIONS_API_KEY is not set")

    if not recipient_addresses:
        print("⚠️  No recipient addresses configured, skipping email.")
        return
    
    personalisation = {
        "environment summary": env_summary
    }
    
    notifications_client = NotificationsAPIClient(NOTIFICATIONS_API_KEY)

    try:
        for recipient in recipient_addresses:
            notifications_client.send_email_notification(
            email_address=recipient.strip(),
            personalisation=personalisation,
            reference="Environment Down Alert",
            template_id=TEMPLATE_ID
        )
    except Exception as e:
        raise Exception(f"Error sending through notifications.service.gov.uk") from e

def load_failures(directory: str = "downloaded-failures"):
    failures = []
    failures_dir = Path(directory)

    if not failures_dir.exists():
        return failures

    for failure_file in failures_dir.glob("*.yml"):
        with open(failure_file) as f:
            data = yaml.safe_load(f) or {}
            if "failures" in data and isinstance(data["failures"], list):
                failures.extend(data["failures"])

    return failures

failures = load_failures()

if not failures:
    print("✅ No failed environments, skipping notification.")
    sys.exit(0)

env_summary = "\n".join(
    f"- {env['name']} ({env['url']}) responded with HTTP {env['status']}"
    for env in failures
)

if not RECIPIENT_ADDRESSES:
    print("⚠️  RECIPIENT_ADDRESSES not set, skipping email.")
    print("Environment summary:\n" + env_summary)
    sys.exit(0)

recipient_addresses = [
    address.strip() 
    for address in RECIPIENT_ADDRESSES.split(";")
    if address.strip()
]

print(
    f"✉️  Sending downtime alert email{'' if len(recipient_addresses) == 1 else 's'} "
    f"to {', '.join(recipient_addresses)}"
)
send_email(recipient_addresses, env_summary)
