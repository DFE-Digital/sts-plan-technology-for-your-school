import argparse
import requests
import json
import time

def make_ids_empty(data):
    if isinstance(data, dict):
        if 'sys' in data and 'id' in data['sys']:
            data['sys']['id'] = ''
        for key in data:
            make_ids_empty(data[key])
    elif isinstance(data, list):
        for item in data:
            make_ids_empty(item)

def modify_interstitial_id(data, new_id):
    interstitial_page = data.get("fields", {}).get("interstitialPage", {})
    if 'en-US' in interstitial_page:
        interstitial_page['en-US']['sys']['id'] = new_id

def main(args):
    headers = {
        "X-Contentful-Topic": "ContentManagement.Entry.publish"
    }
    
    successful_posts = 0
    try:
        with open(args.filename, 'r') as file:
            json_data = json.load(file)

        entries = json_data.get("entries", [])

        if args.content_type:
            entries = [entry for entry in entries if entry.get("sys", {}).get("contentType", {}).get("sys", {}).get("id") == args.content_type]

        for entry in entries:
            if args.make_ids_empty:
                fields = entry.get("fields", {})
                make_ids_empty(fields)
            
            if args.make_interstitial_id_empty:
                modify_interstitial_id(entry, ' ')

            if args.interstitial_id is not None:
                modify_interstitial_id(entry, args.interstitial_id)

            try:
                response = requests.post(args.url, json=entry, headers=headers)
                response.raise_for_status()
                print(f"POST request successful for entry {entry['sys']['id']}")
                successful_posts += 1  

            except requests.exceptions.RequestException as e:
                print(f"POST request failed for entry {entry['sys']['id']}: {str(e)}")

            time.sleep(args.delay)

        print(f"All POST requests completed. {successful_posts} posts were successful.")

    except FileNotFoundError:
        print(f"The '{args.filename}' file was not found.")
    except json.JSONDecodeError:
        print(f"Error parsing JSON data from the '{args.filename}' file.")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Call the Azure Contentful webhook")
    parser.add_argument("filename", help="JSON export of Contentful entries")
    parser.add_argument("url", help="Webhook endpoint URL")
    parser.add_argument("--delay", type=float, default=0.0, help="Delay in seconds between uploads (default: 1.0)")
    parser.add_argument("--content-type", help="Filter entries by content type (optional)")
    parser.add_argument("--make-ids-empty", action='store_true', help="Make all nested ids empty strings in fields (optional)")
    # additional args added only for section migration
    parser.add_argument("--interstitial-id", help="ID value for the interstitialPage.sys.id (optional)")
    parser.add_argument("--make-interstitial-id-empty", action='store_true', help="Make interstitialPage.sys.id an empty string (optional)")

    args = parser.parse_args()
    
    if args.make_interstitial_id_empty and args.interstitial_id:
        print("Error: Both --make-interstitial-empty and --interstitial-id cannot be used together.")
    else:
        main(args)
