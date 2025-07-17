import os
import yaml
from datetime import datetime, timezone
from notifications_python_client.notifications import NotificationsAPIClient
from pathlib import Path

ENVIRONMENT = os.environ.get("ENVIRONMENT") 
NOTIFICATIONS_API_KEY = os.environ.get("NOTIFICATIONS_API_KEY")
RECIPIENT_ADDRESSES = os.environ.get("RECIPIENT_ADDRESSES")

TEMPLATE_ID = "b408feda-d358-4fcc-92fe-b9cd1986e1d8"

WARNING_DAYS = [30, 14, 7, 1]
TODAY = datetime.now(timezone.utc).date()

def send_email(recipient_addresses, secret_expiry_details):
    personalisation = {
        "secret expiry details": secret_expiry_details
    }
    
    notifications_client = NotificationsAPIClient(NOTIFICATIONS_API_KEY)

    try:
        for recipient in recipient_addresses:
            notifications_client.send_email_notification(
                email_address=recipient,
                personalisation=personalisation,
                reference="Secret Expiry Alert",
                template_id=TEMPLATE_ID
            )
    except:
        raise Exception(f"Error sending through notifications.service.gov.uk")

def load_secrets(file_path):
    if not Path(file_path).exists():
        return []
    with open(file_path) as f:
        data = yaml.safe_load(f)
        return data.get("secrets", [])

all_secrets = load_secrets("azure-secrets-expiring.yml")

secret_expiry_details = ""
for secret in all_secrets:
    expiry_date = secret["expires"].date()

    days_left = (expiry_date - TODAY).days
    if days_left in WARNING_DAYS or days_left < 0:
        if days_left < 0:
            secret_expiry_details += f"{secret['name']} expired on {expiry_date}. Please rotate it ASAP.\n"
        else:
            secret_expiry_details += f"{secret['name']} will expire in {days_left} days on {expiry_date}.\n"
        
        if "vault" in secret:
            secret_expiry_details += f"Vault: {secret['vault']}\n"
        
        secret_expiry_details += "\n"

if secret_expiry_details == "":
    exit

recipient_addresses = [address.strip() for address in RECIPIENT_ADDRESSES.split(";") if address.strip()]
print(f"✉️  Sending expiry alert email{'' if len(recipient_addresses) == 1 else 's'}")
send_email(recipient_addresses, secret_expiry_details)
