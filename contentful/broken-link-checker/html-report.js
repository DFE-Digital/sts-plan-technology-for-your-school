/**
 * Generates a self-contained HTML report for the link check results.
 * Designed for non-technical colleagues — uses GOV.UK Design System
 * styling so it reads as an official internal tool.
 */

const STATUS_META = {
  ok: { label: 'Working', tone: 'green' },
  notFound: { label: 'Page not found', tone: 'red' },
  moved: { label: 'URL has moved', tone: 'amber' },
  serverError: { label: 'Server error', tone: 'red' },
  unreachable: { label: "Couldn't reach site", tone: 'red' },
  timeout: { label: 'Took too long', tone: 'red' },
  invalid: { label: 'Invalid URL', tone: 'red' },
  other: { label: 'Other problem', tone: 'amber' },
};

/**
 * Translate a raw check result into a friendly classification.
 * - passing:  { status: <int>, redirected: false }
 * - failing:  { status?: <int>, error?: <name>, redirected?: <bool>, reason?: <str> }
 */
export function classifyResult(result) {
  if (!result) return 'other';
  if (result.valid === true) return 'ok';

  if (result.error === 'AbortError') return 'timeout';
  if (result.error) return 'unreachable';

  if (result.reason === 'Invalid URL format') return 'invalid';

  const s = result.status;
  if (result.redirected) return 'moved';
  if (s === 404 || s === 410) return 'notFound';
  if (typeof s === 'number' && s >= 500) return 'serverError';

  return 'other';
}

function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

function formatDate(date) {
  // e.g. "Monday 20 April 2026, 14:37 BST"
  return date.toLocaleString('en-GB', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    timeZoneName: 'short',
  });
}

/**
 * Build the HTML report.
 *
 * @param {object} args
 * @param {Array} args.externalRows   — every external link instance, with { status, redirected, finalUrl, reason }
 * @param {Array} args.internalRows   — internal links to review
 * @param {string} [args.spaceId]     — Contentful space ID (for deep links)
 * @param {string} [args.environment] — Contentful environment name (default "master")
 * @param {Date}   [args.runAt]       — when the check ran
 * @returns {string} HTML document
 */
export function generateHtmlReport({
  externalRows,
  internalRows,
  spaceId,
  environment = 'master',
  runAt = new Date(),
}) {
  // Pre-compute summary counts for the card strip (so the shell is meaningful
  // even if JS is disabled).
  const totalExternal = externalRows.length;
  const passing = externalRows.filter((r) => classifyResult(r.result) === 'ok').length;
  const failing = totalExternal - passing;
  const pages = new Set(externalRows.map((r) => r.entryId)).size;
  const internalCount = internalRows.length;

  // Attach classification to each row so the client doesn't have to re-derive.
  const rowsForClient = externalRows.map((r) => ({
    entryId: r.entryId,
    internalName: r.internalName,
    uri: r.uri,
    text: r.text,
    richTextPath: r.richTextPath,
    status: r.result?.status ?? null,
    redirected: r.result?.redirected ?? false,
    finalUrl: r.result?.finalUrl ?? null,
    classification: classifyResult(r.result),
  }));

  const internalForClient = internalRows.map((r) => ({
    entryId: r.entryId,
    internalName: r.internalName,
    uri: r.uri,
    text: r.text,
  }));

  const payload = {
    external: rowsForClient,
    internal: internalForClient,
    spaceId: spaceId || null,
    environment,
    runAt: runAt.toISOString(),
  };

  return `<!DOCTYPE html>
<html lang="en-GB">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Link check report — Plan technology for your school</title>
<style>${inlineCss()}</style>
</head>
<body>
<header class="gds-header" role="banner">
  <div class="gds-header__container">
    <div class="gds-header__logo">
      <span class="gds-header__logotype">DfE</span>
    </div>
    <div class="gds-header__content">
      <span class="gds-header__service-name">Plan technology for your school</span>
    </div>
  </div>
</header>

<div class="gds-phase-banner">
  <p class="gds-phase-banner__content">
    <strong class="gds-tag gds-phase-banner__tag">Internal</strong>
    <span class="gds-phase-banner__text">Link check report — not for publication</span>
  </p>
</div>

<main class="gds-main" id="main-content">
  <div class="gds-width-container">

    <h1 class="gds-heading-xl">Link check report</h1>
    <p class="gds-body-l gds-hint">
      Checked <strong>${totalExternal}</strong> external link${totalExternal === 1 ? '' : 's'}
      across <strong>${pages}</strong> page${pages === 1 ? '' : 's'},
      plus <strong>${internalCount}</strong> internal link${internalCount === 1 ? '' : 's'} flagged for review.
    </p>
    <p class="gds-body-s gds-hint" id="run-timestamp">Report generated ${escapeHtml(formatDate(runAt))}</p>

    <div class="gds-summary-grid" aria-label="Summary">
      <div class="gds-summary-card gds-summary-card--green">
        <div class="gds-summary-card__number">${passing}</div>
        <div class="gds-summary-card__label">Working</div>
      </div>
      <div class="gds-summary-card gds-summary-card--red">
        <div class="gds-summary-card__number">${failing}</div>
        <div class="gds-summary-card__label">Need attention</div>
      </div>
      <div class="gds-summary-card gds-summary-card--blue">
        <div class="gds-summary-card__number">${internalCount}</div>
        <div class="gds-summary-card__label">Internal links to review</div>
      </div>
    </div>

    <section class="gds-section" aria-labelledby="needs-attention-heading">
      <h2 class="gds-heading-l" id="needs-attention-heading">Links that need attention</h2>
      <p class="gds-body">
        Each row is one link that failed the automated check. Click
        <strong>Open in Contentful</strong> to go straight to the page where the link lives.
      </p>

      <div class="gds-filter-row">
        <div class="gds-form-group">
          <label class="gds-label" for="search-input">Search</label>
          <input class="gds-input" id="search-input" type="search"
                 placeholder="Search page name, link text, or URL">
        </div>
        <div class="gds-form-group">
          <label class="gds-label" for="type-select">Filter by problem</label>
          <select class="gds-select" id="type-select">
            <option value="">All problems</option>
            <option value="notFound">Page not found</option>
            <option value="moved">URL has moved</option>
            <option value="serverError">Server error</option>
            <option value="unreachable">Couldn't reach site</option>
            <option value="timeout">Took too long</option>
            <option value="invalid">Invalid URL</option>
            <option value="other">Other problem</option>
          </select>
        </div>
        <div class="gds-filter-count" id="filter-count"></div>
      </div>

      <div class="gds-table-wrapper">
        <table class="gds-table" id="failed-table">
          <caption class="gds-visually-hidden">Links that need attention</caption>
          <thead>
            <tr>
              <th scope="col">Page</th>
              <th scope="col">Link text</th>
              <th scope="col">URL</th>
              <th scope="col">Problem</th>
              <th scope="col" class="gds-table__cell--actions">Action</th>
            </tr>
          </thead>
          <tbody id="failed-tbody"></tbody>
        </table>
        <p class="gds-empty-state" id="failed-empty" hidden>
          No links match the current filter.
        </p>
      </div>
    </section>

    <section class="gds-section" aria-labelledby="internal-heading">
      <details class="gds-details">
        <summary class="gds-details__summary">
          <h2 class="gds-heading-m gds-details__summary-heading" id="internal-heading">
            Internal links to review manually (${internalCount})
          </h2>
        </summary>
        <div class="gds-details__text">
          <p class="gds-body">
            These point to internal or partial URLs that the automatic checker can't
            verify on its own. Worth a human eye to confirm they still land on the
            right place.
          </p>
          <div class="gds-table-wrapper">
            <table class="gds-table" id="internal-table">
              <thead>
                <tr>
                  <th scope="col">Page</th>
                  <th scope="col">Link text</th>
                  <th scope="col">URL</th>
                  <th scope="col" class="gds-table__cell--actions">Action</th>
                </tr>
              </thead>
              <tbody id="internal-tbody"></tbody>
            </table>
          </div>
        </div>
      </details>
    </section>

    <section class="gds-section" aria-labelledby="passing-heading">
      <details class="gds-details">
        <summary class="gds-details__summary">
          <h2 class="gds-heading-m gds-details__summary-heading" id="passing-heading">
            All working external links (${passing})
          </h2>
        </summary>
        <div class="gds-details__text">
          <p class="gds-body">
            For completeness — these links passed the check and don't need action.
          </p>
          <div class="gds-table-wrapper">
            <table class="gds-table" id="passing-table">
              <thead>
                <tr>
                  <th scope="col">Page</th>
                  <th scope="col">Link text</th>
                  <th scope="col">URL</th>
                </tr>
              </thead>
              <tbody id="passing-tbody"></tbody>
            </table>
          </div>
        </div>
      </details>
    </section>

    <section class="gds-section gds-section--footer" aria-labelledby="about-heading">
      <h2 class="gds-heading-s" id="about-heading">About this report</h2>
      <p class="gds-body-s">
        Generated automatically by the link checker. Redirects are flagged as
        "URL has moved" — the destination still works, but the content should be
        updated to point to the new location. Some sites return
        <code>403</code>, <code>406</code>, or <code>429</code> when probed by
        an automated tool; these are treated as working and don't appear above.
      </p>
    </section>

  </div>
</main>

<script id="report-data" type="application/json">${escapeJsonForScript(JSON.stringify(payload))}</script>
<script>${inlineJs()}</script>
</body>
</html>`;
}

/**
 * JSON embedded in a <script> tag must escape </script> sequences and
 * line separators that can break HTML parsing.
 */
function escapeJsonForScript(json) {
  return json
    .replace(/</g, '\\u003c')
    .replace(/>/g, '\\u003e')
    .replace(/&/g, '\\u0026')
    .replace(/\u2028/g, '\\u2028')
    .replace(/\u2029/g, '\\u2029');
}

function inlineCss() {
  // GOV.UK Design System palette and type scale, trimmed to what's used here.
  return `
    :root {
      --gds-black: #0b0c0c;
      --gds-white: #ffffff;
      --gds-blue: #1d70b8;
      --gds-dark-blue: #003078;
      --gds-light-blue: #5694ca;
      --gds-light-grey: #f3f2f1;
      --gds-mid-grey: #b1b4b6;
      --gds-dark-grey: #505a5f;
      --gds-red: #d4351c;
      --gds-green: #00703c;
      --gds-yellow: #ffdd00;
      --gds-amber: #f47738;
      --gds-purple: #4c2c92;
    }
    * { box-sizing: border-box; }
    html { font-size: 16px; }
    body {
      margin: 0;
      font-family: "GDS Transport", Arial, sans-serif;
      color: var(--gds-black);
      background: var(--gds-white);
      line-height: 1.5;
      -webkit-font-smoothing: antialiased;
    }
    a { color: var(--gds-blue); text-decoration: underline; text-decoration-thickness: 1px; text-underline-offset: 0.1em; }
    a:hover { color: var(--gds-dark-blue); text-decoration-thickness: 3px; }
    a:focus { outline: 3px solid transparent; background: var(--gds-yellow); color: var(--gds-black); box-shadow: 0 -2px var(--gds-yellow), 0 4px var(--gds-black); text-decoration: none; }
    code { font-family: ui-monospace, "Consolas", monospace; background: var(--gds-light-grey); padding: 1px 4px; border-radius: 2px; font-size: 0.95em; }

    /* Header */
    .gds-header { background: var(--gds-black); color: var(--gds-white); border-bottom: 10px solid var(--gds-blue); }
    .gds-header__container { max-width: 1200px; margin: 0 auto; padding: 15px 30px; display: flex; align-items: center; gap: 24px; flex-wrap: wrap; }
    .gds-header__logotype { font-size: 30px; font-weight: 700; letter-spacing: 0.5px; }
    .gds-header__service-name { font-size: 19px; font-weight: 700; }

    /* Phase banner */
    .gds-phase-banner { background: var(--gds-white); border-bottom: 1px solid var(--gds-mid-grey); }
    .gds-phase-banner__content { max-width: 1200px; margin: 0 auto; padding: 10px 30px; font-size: 16px; }
    .gds-phase-banner__text { margin-left: 10px; }

    .gds-tag { display: inline-block; padding: 4px 8px; font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 1px; color: var(--gds-white); background: var(--gds-blue); border-radius: 0; }
    .gds-tag--red { background: var(--gds-red); }
    .gds-tag--green { background: var(--gds-green); }
    .gds-tag--amber { background: var(--gds-amber); }
    .gds-tag--grey { background: var(--gds-dark-grey); }

    /* Layout */
    .gds-main { padding: 40px 0 80px; }
    .gds-width-container { max-width: 1200px; margin: 0 auto; padding: 0 30px; }

    /* Typography */
    .gds-heading-xl { font-size: 48px; line-height: 1.09; font-weight: 700; margin: 0 0 30px; }
    .gds-heading-l  { font-size: 36px; line-height: 1.11; font-weight: 700; margin: 40px 0 20px; }
    .gds-heading-m  { font-size: 24px; line-height: 1.25; font-weight: 700; margin: 0; }
    .gds-heading-s  { font-size: 19px; line-height: 1.31; font-weight: 700; margin: 30px 0 15px; }
    .gds-body-l { font-size: 24px; line-height: 1.33; margin: 0 0 20px; }
    .gds-body   { font-size: 19px; line-height: 1.47; margin: 0 0 20px; }
    .gds-body-s { font-size: 16px; line-height: 1.5;  margin: 0 0 15px; }
    .gds-hint { color: var(--gds-dark-grey); }

    @media (max-width: 640px) {
      .gds-heading-xl { font-size: 32px; }
      .gds-heading-l  { font-size: 24px; }
      .gds-body-l { font-size: 19px; }
      .gds-body   { font-size: 16px; }
    }

    /* Summary cards */
    .gds-summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0; margin: 30px 0 50px; border: 1px solid var(--gds-mid-grey); }
    .gds-summary-card { padding: 20px 25px; border-left: 10px solid var(--gds-dark-grey); background: var(--gds-white); border-right: 1px solid var(--gds-mid-grey); }
    .gds-summary-card:last-child { border-right: none; }
    .gds-summary-card--green { border-left-color: var(--gds-green); }
    .gds-summary-card--red   { border-left-color: var(--gds-red); }
    .gds-summary-card--amber { border-left-color: var(--gds-amber); }
    .gds-summary-card--blue  { border-left-color: var(--gds-blue); }
    .gds-summary-card__number { font-size: 48px; font-weight: 700; line-height: 1; margin-bottom: 4px; }
    .gds-summary-card__label  { font-size: 16px; color: var(--gds-dark-grey); }

    @media (max-width: 640px) {
      .gds-summary-card { border-right: none; border-bottom: 1px solid var(--gds-mid-grey); }
      .gds-summary-card:last-child { border-bottom: none; }
      .gds-summary-card__number { font-size: 36px; }
    }

    /* Sections */
    .gds-section { margin: 50px 0; }
    .gds-section--footer { margin-top: 60px; padding-top: 30px; border-top: 1px solid var(--gds-mid-grey); color: var(--gds-dark-grey); }

    /* Filter row */
    .gds-filter-row { display: flex; gap: 20px; align-items: flex-end; flex-wrap: wrap; margin: 20px 0 25px; padding: 20px; background: var(--gds-light-grey); border-left: 5px solid var(--gds-blue); }
    .gds-form-group { flex: 1; min-width: 200px; }
    .gds-label { display: block; font-size: 19px; font-weight: 700; margin-bottom: 5px; }
    .gds-input, .gds-select { width: 100%; padding: 8px 10px; font-size: 19px; line-height: 1.25; border: 2px solid var(--gds-black); border-radius: 0; background: var(--gds-white); font-family: inherit; }
    .gds-input:focus, .gds-select:focus { outline: 3px solid var(--gds-yellow); outline-offset: 0; box-shadow: inset 0 0 0 2px; }
    .gds-filter-count { font-size: 16px; color: var(--gds-dark-grey); white-space: nowrap; padding-bottom: 10px; }

    /* Tables */
    .gds-table-wrapper { overflow-x: auto; }
    .gds-table { width: 100%; border-collapse: collapse; font-size: 16px; margin-bottom: 20px; }
    .gds-table caption { text-align: left; font-weight: 700; margin-bottom: 10px; }
    .gds-table thead { border-bottom: 2px solid var(--gds-black); }
    .gds-table th { text-align: left; padding: 12px 20px 12px 0; font-weight: 700; vertical-align: top; }
    .gds-table td { padding: 12px 20px 12px 0; border-bottom: 1px solid var(--gds-mid-grey); vertical-align: top; }
    .gds-table tr:hover td { background: var(--gds-light-grey); }
    .gds-table__cell--actions { white-space: nowrap; }
    .gds-table .page-name { font-weight: 700; max-width: 260px; }
    .gds-table .link-text { max-width: 220px; color: var(--gds-dark-grey); font-style: italic; }
    .gds-table .link-text::before { content: "\\201C"; }
    .gds-table .link-text::after  { content: "\\201D"; }
    .gds-table .link-url { font-family: ui-monospace, "Consolas", monospace; font-size: 14px; word-break: break-all; max-width: 380px; }
    .gds-table .link-url a { display: inline-block; max-width: 100%; }
    .gds-table .moved-note { display: block; font-size: 13px; color: var(--gds-dark-grey); margin-top: 4px; font-family: ui-monospace, monospace; word-break: break-all; }
    .gds-table .moved-note strong { font-family: Arial, sans-serif; font-size: 14px; color: var(--gds-black); }

    /* Action buttons */
    .gds-button { display: inline-block; padding: 6px 12px; font-size: 14px; font-weight: 700; color: var(--gds-white); background: var(--gds-green); border: 2px solid transparent; border-radius: 0; text-decoration: none; cursor: pointer; font-family: inherit; box-shadow: 0 2px 0 #002d18; margin-bottom: 4px; }
    .gds-button:hover { background: #005a30; color: var(--gds-white); text-decoration: none; }
    .gds-button:focus { outline: 3px solid var(--gds-yellow); outline-offset: 0; }
    .gds-button--secondary { background: var(--gds-white); color: var(--gds-black); border-color: var(--gds-black); box-shadow: 0 2px 0 var(--gds-black); }
    .gds-button--secondary:hover { background: var(--gds-light-grey); color: var(--gds-black); }

    /* Details / collapsibles */
    .gds-details { margin: 20px 0; }
    .gds-details__summary { cursor: pointer; color: var(--gds-blue); padding: 10px 0; list-style: none; }
    .gds-details__summary::-webkit-details-marker { display: none; }
    .gds-details__summary::before { content: "\\25B6"; display: inline-block; margin-right: 10px; transition: transform 0.15s; font-size: 0.7em; }
    .gds-details[open] .gds-details__summary::before { transform: rotate(90deg); }
    .gds-details__summary:hover { color: var(--gds-dark-blue); }
    .gds-details__summary-heading { display: inline; color: inherit; text-decoration: underline; text-decoration-thickness: 1px; text-underline-offset: 0.1em; }
    .gds-details__summary:hover .gds-details__summary-heading { text-decoration-thickness: 3px; }
    .gds-details__text { padding: 15px 0 0 25px; border-left: 5px solid var(--gds-mid-grey); margin-left: 5px; padding-left: 20px; }

    .gds-empty-state { padding: 20px; background: var(--gds-light-grey); font-size: 19px; text-align: center; }

    .gds-visually-hidden { position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0; }
  `;
}

function inlineJs() {
  // Client-side rendering + filtering. Reads embedded JSON and paints tables.
  // Uses textContent / attribute APIs throughout — no innerHTML with untrusted data.
  return `
  (function() {
    const data = JSON.parse(document.getElementById('report-data').textContent);
    const STATUS_LABELS = ${JSON.stringify(
      Object.fromEntries(Object.entries(STATUS_META).map(([k, v]) => [k, v])),
    )};

    const contentfulUrl = (entryId) => {
      if (!data.spaceId) return null;
      return 'https://app.contentful.com/spaces/' + encodeURIComponent(data.spaceId)
        + '/environments/' + encodeURIComponent(data.environment || 'master')
        + '/entries/' + encodeURIComponent(entryId);
    };

    // Split external rows into failing / passing
    const failingRows = data.external.filter(r => r.classification !== 'ok');
    const passingRows = data.external.filter(r => r.classification === 'ok');

    function makeTag(classification) {
      const meta = STATUS_LABELS[classification] || STATUS_LABELS.other;
      const span = document.createElement('span');
      span.className = 'gds-tag gds-tag--' + (meta.tone === 'red' ? 'red' : meta.tone === 'green' ? 'green' : meta.tone === 'amber' ? 'amber' : 'grey');
      span.textContent = meta.label;
      return span;
    }

    function makeContentfulButton(entryId, label) {
      const href = contentfulUrl(entryId);
      if (!href) {
        const span = document.createElement('span');
        span.className = 'gds-hint';
        span.style.fontSize = '13px';
        span.textContent = entryId;
        return span;
      }
      const a = document.createElement('a');
      a.className = 'gds-button';
      a.href = href;
      a.target = '_blank';
      a.rel = 'noopener noreferrer';
      a.textContent = label || 'Open in Contentful';
      return a;
    }

    function makeUrlCell(uri, finalUrl) {
      const cell = document.createElement('td');
      cell.className = 'link-url';

      const a = document.createElement('a');
      a.href = uri;
      a.target = '_blank';
      a.rel = 'noopener noreferrer';
      a.textContent = uri;
      cell.appendChild(a);

      if (finalUrl && finalUrl !== uri) {
        const note = document.createElement('span');
        note.className = 'moved-note';
        const strong = document.createElement('strong');
        strong.textContent = 'Now redirects to: ';
        note.appendChild(strong);
        const target = document.createElement('a');
        target.href = finalUrl;
        target.target = '_blank';
        target.rel = 'noopener noreferrer';
        target.textContent = finalUrl;
        note.appendChild(target);
        cell.appendChild(note);
      }
      return cell;
    }

    function renderFailingRow(row) {
      const tr = document.createElement('tr');

      const nameTd = document.createElement('td');
      nameTd.className = 'page-name';
      nameTd.textContent = row.internalName || '(unnamed)';
      tr.appendChild(nameTd);

      const textTd = document.createElement('td');
      textTd.className = 'link-text';
      textTd.textContent = row.text || '';
      tr.appendChild(textTd);

      tr.appendChild(makeUrlCell(row.uri, row.finalUrl));

      const problemTd = document.createElement('td');
      problemTd.appendChild(makeTag(row.classification));
      tr.appendChild(problemTd);

      const actionTd = document.createElement('td');
      actionTd.className = 'gds-table__cell--actions';
      actionTd.appendChild(makeContentfulButton(row.entryId));
      tr.appendChild(actionTd);

      return tr;
    }

    function renderInternalRow(row) {
      const tr = document.createElement('tr');

      const nameTd = document.createElement('td');
      nameTd.className = 'page-name';
      nameTd.textContent = row.internalName || '(unnamed)';
      tr.appendChild(nameTd);

      const textTd = document.createElement('td');
      textTd.className = 'link-text';
      textTd.textContent = row.text || '';
      tr.appendChild(textTd);

      tr.appendChild(makeUrlCell(row.uri));

      const actionTd = document.createElement('td');
      actionTd.className = 'gds-table__cell--actions';
      actionTd.appendChild(makeContentfulButton(row.entryId));
      tr.appendChild(actionTd);

      return tr;
    }

    function renderPassingRow(row) {
      const tr = document.createElement('tr');

      const nameTd = document.createElement('td');
      nameTd.className = 'page-name';
      nameTd.textContent = row.internalName || '(unnamed)';
      tr.appendChild(nameTd);

      const textTd = document.createElement('td');
      textTd.className = 'link-text';
      textTd.textContent = row.text || '';
      tr.appendChild(textTd);

      const urlTd = document.createElement('td');
      urlTd.className = 'link-url';
      const a = document.createElement('a');
      a.href = row.uri;
      a.target = '_blank';
      a.rel = 'noopener noreferrer';
      a.textContent = row.uri;
      urlTd.appendChild(a);
      tr.appendChild(urlTd);

      return tr;
    }

    // Initial render
    const failedTbody = document.getElementById('failed-tbody');
    const internalTbody = document.getElementById('internal-tbody');
    const passingTbody = document.getElementById('passing-tbody');

    // Sort failing rows so same page groups together
    failingRows.sort((a, b) => (a.internalName || '').localeCompare(b.internalName || ''));
    passingRows.sort((a, b) => (a.internalName || '').localeCompare(b.internalName || ''));
    data.internal.sort((a, b) => (a.internalName || '').localeCompare(b.internalName || ''));

    passingRows.forEach(r => passingTbody.appendChild(renderPassingRow(r)));
    data.internal.forEach(r => internalTbody.appendChild(renderInternalRow(r)));

    // Filtering for failing rows
    const searchInput = document.getElementById('search-input');
    const typeSelect = document.getElementById('type-select');
    const filterCount = document.getElementById('filter-count');
    const emptyState = document.getElementById('failed-empty');

    function applyFilter() {
      const q = searchInput.value.trim().toLowerCase();
      const t = typeSelect.value;

      // Remove existing rows
      while (failedTbody.firstChild) failedTbody.removeChild(failedTbody.firstChild);

      const filtered = failingRows.filter(r => {
        if (t && r.classification !== t) return false;
        if (!q) return true;
        const hay = ((r.internalName || '') + ' ' + (r.text || '') + ' ' + (r.uri || '')).toLowerCase();
        return hay.includes(q);
      });

      filtered.forEach(r => failedTbody.appendChild(renderFailingRow(r)));

      emptyState.hidden = filtered.length > 0;
      filterCount.textContent = filtered.length === failingRows.length
        ? 'Showing all ' + filtered.length + ' issue' + (filtered.length === 1 ? '' : 's')
        : 'Showing ' + filtered.length + ' of ' + failingRows.length + ' issues';
    }

    searchInput.addEventListener('input', applyFilter);
    typeSelect.addEventListener('change', applyFilter);
    applyFilter();
  })();
  `;
}
