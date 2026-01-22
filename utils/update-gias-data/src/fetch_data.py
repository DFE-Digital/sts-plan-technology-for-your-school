from logging import getLogger
from playwright.sync_api import Playwright, sync_playwright, TimeoutError as PlaywrightTimeoutError

from src.constants import DOWNLOAD_PATH, GIAS_DATA_URL

logger = getLogger(__name__)

DEFAULT_TIMEOUT_MS = 120_000


def _fetch_gias_data(play: Playwright) -> None:
    """Use playwright to download the dynamically generated GIAS data file"""
    browser = play.webkit.launch(headless=True)

    context = browser.new_context(accept_downloads=True)
    context.set_default_timeout(DEFAULT_TIMEOUT_MS)

    page = context.new_page()

    logger.info("Navigating to %s", GIAS_DATA_URL)
    page.goto(GIAS_DATA_URL, wait_until="domcontentloaded")
    page.wait_for_load_state("networkidle")

    # Ensure checkbox is present then tick it
    page.locator("#all-group-records-with-linkszip-checkbox").wait_for(state="visible")
    page.locator("#all-group-records-with-linkszip-checkbox").check()

    # Click "Download selected files"
    page.get_by_role("button", name="Download selected files").click()

    # Wait for the dynamically generated Results.zip button to appear and be clickable
    results_btn = page.locator('input#download-button[value="Results.zip"]')
    logger.info("Waiting for Results.zip button to appear")
    results_btn.wait_for(state="visible")
    expect_enabled_timeout_ms = DEFAULT_TIMEOUT_MS

    # Sometimes it appears disabled briefly while server-side job runs
    page.wait_for_function(
        "btn => !btn.disabled",
        arg=results_btn.element_handle(),
        timeout=expect_enabled_timeout_ms,
    )

    logger.info("Requesting download")

    # IMPORTANT: expect_download must wrap the click that triggers the download
    with page.expect_download(timeout=DEFAULT_TIMEOUT_MS) as download_info:
        results_btn.click()

    download = download_info.value

    logger.info("Downloading results")
    download.save_as(DOWNLOAD_PATH / "extract.zip")

    logger.info("Finished downloading file")

    context.close()
    browser.close()


def fetch_and_save_gias_data() -> None:
    """Fetch and save GIAS data in zip format"""
    with sync_playwright() as playwright:
        _fetch_gias_data(playwright)
