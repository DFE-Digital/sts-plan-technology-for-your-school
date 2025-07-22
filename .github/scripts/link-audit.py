#!/usr/bin/env python3
"""
Check every external link found in
1. All entries in a Contentful space
2. The DfE guidance page on GOV.UK

• Fails (exit code 1) if any dead links are found, so the CI job turns red.
• Prints a table of broken URLs for quick triage.
"""

import json
import os
import re
import sys
from urllib.parse import urljoin

import requests
from bs4 import BeautifulSoup
from contentful import Client
from time import sleep

# Configuration
GUIDANCE_BASE_URL = "https://www.gov.uk"
GUIDANCE_SLUG = "/guidance/meeting-digital-and-technology-standards-in-schools-and-colleges"
EXTERNAL_LINK_REGEX = re.compile(r"https?://[^\s)\"'>]+")

CDA_TOKEN = os.environ["CONTENTFUL_CDA_TOKEN"]
ENVIRONMENT = os.environ["CONTENTFUL_ENVIRONMENT"]
SPACE_ID = os.environ["CONTENTFUL_SPACE_ID"]
INCLUDE_GUIDANCE = os.environ.get("INCLUDE_GUIDANCE", "false").lower() == "true"

# --- HTTP link checking ---

def is_alive(url, timeout=15):
    if url.startswith('#') or url.startswith('mailto'):
        return True

    headers = {
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
        "Accept-Encoding": "gzip, deflate, br, zstd",
        "Accept-Language": "en-GB,en;q=0.9,en-US;q=0.8",
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36 Edg/138.0.0.0"
    }

    try:
        r = requests.head(url, headers=headers, allow_redirects=True, timeout=timeout)
        if r.status_code in (403, 404, 405, 501):
            r = requests.get(url, headers=headers, allow_redirects=True, timeout=timeout, stream=True)
        #print(f"{url} returned status code {r.status_code}")
        return r.status_code < 400
    except requests.RequestException:
        return False


# --- Helpers ---

def normalize_url(url):
    return urljoin(GUIDANCE_BASE_URL, url) if url.startswith("/") else url


def normalize_links_map(link_map):
    normalized = {}
    for url, sources in link_map.items():
        cleaned = normalize_url(url)
        normalized.setdefault(cleaned, set()).update(sources)
    return normalized


def report_dead_links(label, link_map):
    print(f"\nChecking {len(link_map)} {label} links. Please wait...")
    dead_links = {}

    for url, sources in sorted(link_map.items()):
        if not is_alive(url):
            dead_links[url] = sources

    if dead_links:
        print(f"\n❌ {len(dead_links.keys())} possible dead link(s) found in {label}. Please check these manually as it may just be that the request requires a cookie in the header.")
        for url, sources in dead_links.items():
            print(f"\n- {url}")
            for source in sorted(sources):
                print(f"   ↳ found on {source}")
        return True
    else:
        print(f"✅ No dead links detected in {label}")
        return False


# --- Contentful crawling ---

def all_entries(client, page_size=25):
    skip = 0
    while True:
        sleep(0.2)
        print(f"Pulling page {(skip // page_size) + 1} of Contentful entries", flush=True)

        try:
            page = client.entries({"limit": page_size, "skip": skip, "include": 0})
        except Exception as e:
            print(f"❌ Failed to fetch entries at skip={skip}: {e}")
            break

        if not page:
            print("No page found")
            break

        for entry in page:
            yield entry

        if len(page) < page_size:
            break

        skip += page_size


def gather_links_from_contentful():
    client = Client(SPACE_ID, CDA_TOKEN, environment=ENVIRONMENT)
    link_map = {}

    print("\nGathering links from Contentful entries")

    for entry in all_entries(client):
        entry_id = entry.raw["sys"]["id"]
        try:
            blob = json.dumps(entry.raw, default=str)
            found_links = EXTERNAL_LINK_REGEX.findall(blob)
            for url in found_links:
                link_map.setdefault(url.replace("\\", "/"), set()).add(f"https://app.contentful.com/spaces/{SPACE_ID}/environments/master/entries/{entry_id}")
                
        except Exception as e:
            print(f"❌ Error processing entry {entry_id}: {e}")
            print(json.dumps(entry.raw, indent=2, default=str))
            sys.exit(1)

    return link_map


# --- GOV.UK guidance crawling ---

def gather_links_from_guidance():
    start_url = urljoin(GUIDANCE_BASE_URL, GUIDANCE_SLUG)
    seen = set()
    link_map = {}

    try:
        start_html = requests.get(start_url, timeout=20).text
    except requests.RequestException as e:
        print(f"Failed to fetch start page: {e}")
        return {}

    soup = BeautifulSoup(start_html, "html.parser")
    nav_links = [
        urljoin(GUIDANCE_BASE_URL, a["href"])
        for a in soup.find_all("a", class_="govuk-link", href=True)
    ]

    print("\nGathering links from guidance pages")

    for href in nav_links:
        if href in seen:
            continue
        seen.add(href)
        try:
            page_html = requests.get(href, timeout=20).text
            page_soup = BeautifulSoup(page_html, "html.parser")
            for a in page_soup.find_all("a", href=True):
                url = a["href"]
                if url.startswith("http://") or url.startswith("https://") or url.startswith("/"):
                    link_map.setdefault(url, set()).add(href)
        except requests.RequestException as e:
            print(f"❌ Failed to fetch {href}: {e}")

    return link_map


# --- Entry point ---

def main():
    print(f"Checking Contentful space {SPACE_ID}, environment {ENVIRONMENT}")

    contentful_links = normalize_links_map(gather_links_from_contentful())
    report_dead_links("Contentful", contentful_links)

    if INCLUDE_GUIDANCE:
        guidance_links = normalize_links_map(gather_links_from_guidance())
        report_dead_links("GOV.UK guidance", guidance_links)

if __name__ == "__main__":
    main()
