const STORAGE_KEY = "BrowserHistory";
export const DEFAULT_ROUTE = "/self-assessment";
export const BACK_BUTTON_ID = "back-button-link";

/**
 * Tracks a users browser history, stores it in local storage, and amends the back button link (if existing on page)
 */
export class BrowserHistory {
    history = [];

    get lastUrl() {
        if (this.history.length > 0) {
            return this.history[this.history.length - 1];
        }

        return DEFAULT_ROUTE;
    }

    constructor() {
        this.history = this.getHistory();

        if (!this.ifNavigatingBackwardsRemoveUrl()) {
            this.amendBackButton();
            this.addUrl();
        }
        else {
            this.amendBackButton();
        }
    }

    /**
     * Adds current window href to history
     */
    addUrl() {
        this.history.push(window.location.href);
        this.saveHistory();
    }

    /**
     * Checks to see if we are navigating backwards, if so removes URL(s) from history
     * @returns {boolean} Whether we are navigating backwards or not
     */
    ifNavigatingBackwardsRemoveUrl() {
        const indexOfHrefInHistory = this.history.findIndex(url => url === window.location.href);

        if (indexOfHrefInHistory == -1 || indexOfHrefInHistory < this.history.length - 2) {
            return false;
        }

        this.history = this.history.slice(0, indexOfHrefInHistory);
        this.saveHistory();

        return true;
    }

    getHistory() {
        if (this.shouldClearHistory()) {
            this.clearHistory();
        }

        const fromStorage = localStorage.getItem(STORAGE_KEY);

        if (fromStorage) {
            return JSON.parse(fromStorage);
        }

        return [];
    }

    saveHistory() {
        const stringified = JSON.stringify(this.history);
        localStorage.setItem(STORAGE_KEY, stringified);
    }

    clearHistory() {
        this.history = [];
        this.saveHistory();
    }

    /**
     * Gets the back button link from the page, if it exists amend HREF to be last url in history
     */
    amendBackButton() {
        const backButtonLink = document.getElementById(BACK_BUTTON_ID);

        if (!backButtonLink) {
            return;
        }

        backButtonLink.setAttribute("href", this.lastUrl);
    }

    shouldClearHistory() {
        return window.location.pathname == "/" || window.location.pathname == DEFAULT_ROUTE;
    }
}