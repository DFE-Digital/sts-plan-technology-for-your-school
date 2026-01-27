import {
  BrowserHistory,
  DEFAULT_ROUTE,
  BACK_BUTTON_ID,
} from '../../../src/Dfe.PlanTech.Web.Node/scripts/browser-history';

let windowSpy;

describe('BrowserHistory tests', () => {
  beforeEach(() => {
    windowSpy = jest.spyOn(window, 'window', 'get');
  });

  afterEach(() => {
    windowSpy.mockRestore();
    localStorage.clear();
  });

  test('constructs', () => {
    setWindowSpyLocation({
      href: 'http://www.plan-tech.com/',
      pathname: '/',
      hostname: 'www.plan-tech.com',
      protocol: 'http',
    });

    const history = new BrowserHistory();

    expect(history).toBeTruthy();
  });

  test('returns default route if no history', () => {
    const history = new BrowserHistory();

    expect(history).toBeTruthy();

    history.clearHistory();

    expect(history.lastUrl).toEqual(DEFAULT_ROUTE);
  });

  test('saves url to history', () => {
    const href = 'http://www.plan-tech.com/';

    setWindowSpyLocation({
      href,
      pathname: '/',
      hostname: 'www.plan-tech.com',
      protocol: 'http',
    });

    const browserHistory = new BrowserHistory();

    const history = browserHistory.history;

    expect(history).toHaveLength(1);
    expect(history[0]).toEqual(href);
  });

  test('saves multiple to history', () => {
    const pathNames = ['first-url', 'second-url', 'third-url'];
    const protocol = 'http';
    const hostName = 'www.plan-tech.com';

    for (let x = 0; x < pathNames.length; x++) {
      const path = pathNames[x];

      const href = `${protocol}://${hostName}/${path}`;
      setWindowSpyLocation({
        href: href,
        pathname: path,
        hostname: hostName,
        protocol,
      });

      const browserHistory = new BrowserHistory();

      const history = browserHistory.history;

      expect(history).toHaveLength(x + 1);
      expect(history[x]).toEqual(href);
    }
  });

  test('removes from history if navigating backwards', () => {
    const pathNames = ['first-url', 'second-url', 'third-url'];
    const protocol = 'http';
    const hostName = 'www.plan-tech.com';

    for (const path of pathNames) {
      const href = `${protocol}://${hostName}/${path}`;
      setWindowSpyLocation({
        href: href,
        pathname: path,
        hostname: hostName,
        protocol,
      });

      const history = new BrowserHistory();
      expect(history).toBeTruthy();
    }

    const href = `${protocol}://${hostName}/${pathNames[1]}`;
    setWindowSpyLocation({
      href: href,
      pathname: pathNames[1],
      hostname: hostName,
      protocol,
    });

    const browserHistory = new BrowserHistory();
    const history = browserHistory.history;
    expect(history).toHaveLength(2);
    expect(history[0]).toEqual(`${protocol}://${hostName}/${pathNames[0]}`);
  });

  test("doesn't add to history if identical", () => {
    const pathNames = ['first-url', 'second-url', 'third-url'];
    const protocol = 'http';
    const hostName = 'www.plan-tech.com';

    for (const path of pathNames) {
      const href = `${protocol}://${hostName}/${path}`;
      setWindowSpyLocation({
        href: href,
        pathname: path,
        hostname: hostName,
        protocol,
      });

      const history = new BrowserHistory();
      expect(history).toBeTruthy();
    }

    const href = `${protocol}://${hostName}/${pathNames[2]}`;
    setWindowSpyLocation({
      href: href,
      pathname: pathNames[2],
      hostname: hostName,
      protocol,
    });

    const browserHistory = new BrowserHistory();
    const history = browserHistory.history;
    expect(history).toHaveLength(3);
    expect(history[0]).toEqual(`${protocol}://${hostName}/${pathNames[0]}`);
  });

  test('sets back link href', () => {
    document.body.innerHTML = `<a id="${BACK_BUTTON_ID}">Back</A>`;

    const href = 'http://www.plan-tech.com/test';
    setWindowSpyLocation({
      href,
      pathname: '/test',
      hostname: 'www.plan-tech.com',
      protocol: 'http',
    });

    new BrowserHistory();

    const link = document.getElementById(BACK_BUTTON_ID);

    let hrefValue = link.getAttribute('href');

    expect(hrefValue).toBeTruthy();
    expect(hrefValue).toEqual(DEFAULT_ROUTE);
    setWindowSpyLocation({
      href: href + 'hi',
      pathname: '/testhi',
      hostname: 'www.plan-tech.com',
      protocol: 'http',
    });

    new BrowserHistory();
    hrefValue = link.getAttribute('href');
    expect(hrefValue).toEqual(href);
  });

  /**
   * Set the window.locaton value in the window spy
   * @param {WindowLocation} location
   */
  const setWindowSpyLocation = (location) => {
    windowSpy.mockImplementation(() => ({
      location,
    }));
  };
});
/**
 * window.location
 * @typedef {Object} WindowLocation
 * @property {string} href
 * @property {string} hostname
 * @property {string} pathname
 * @property {string} protocol
 */
