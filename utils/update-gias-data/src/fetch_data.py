from logging import getLogger

from playwright.sync_api import Playwright, sync_playwright

from src.constants import DOWNLOAD_PATH, GIAS_DATA_URL

logger = getLogger(__name__)


def _fetch_gias_data(play: Playwright) -> None:
    """Use playwright to download the dynamically generated GIAS data file"""
    browser = play.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    logger.info("Navigating to %s", GIAS_DATA_URL)
    page.goto(GIAS_DATA_URL)

    page.locator("#all-group-records-with-linkszip-checkbox").check()
    page.get_by_role("button", name="Download selected files").click()

    logger.info("Requesting download")
    page.get_by_role("button", name="Results.zip").click()

    logger.info("Downloading results")
    with page.expect_download() as download_info:
        download = download_info.value
        download.save_as(DOWNLOAD_PATH / "extract.zip")

    logger.info("Finished downloading file")

    context.close()
    browser.close()


def fetch_and_save_gias_data() -> None:
    """Fetch and save GIAS data in zip format"""
    with sync_playwright() as playwright:
        _fetch_gias_data(playwright)
