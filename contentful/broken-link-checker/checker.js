import fs from 'fs';
import path from 'path';
import 'dotenv/config';

import { generateHtmlReport } from './html-report.js';

const PLANTECH_BASE_URL = 'https://www.plan-technology-for-your-school.education.gov.uk';
const LINKS_FILE_PATH = path.resolve('./result/links.json');
const REQUEST_TIMEOUT = 15000;

const CSV_REPORT_DIR = path.resolve('./report');
const EXTERNAL_LINK_CSV_PATH = path.join(CSV_REPORT_DIR, 'failed-external-links.csv');
const INTERNAL_LINK_CSV_PATH = path.join(CSV_REPORT_DIR, 'internal-links-to-check.csv');

const HTML_REPORT_PATH = path.join(CSV_REPORT_DIR, 'report.html');

const CONTENTFUL_SPACE_ID = process.env.SPACE_ID || null;
const CONTENTFUL_ENVIRONMENT = process.env.CONTENTFUL_ENVIRONMENT || 'master';

function isValidUrl(uri) {
  try {
    new URL(uri);
    return true;
  } catch {
    return false;
  }
}

function readLinksData() {
  if (!fs.existsSync(LINKS_FILE_PATH)) {
    console.error(`Links file does not exist at ${LINKS_FILE_PATH}`);
    process.exit(1);
  }

  try {
    return JSON.parse(fs.readFileSync(LINKS_FILE_PATH, 'utf8'));
  } catch (e) {
    console.error('Failed to read/parse links file:', e.message);
    process.exit(1);
  }
}

function categoriseLinks(data) {
  const internalLinks = [];
  const externalLinks = [];

  for (const link of data) {
    link.uri = link.uri.startsWith('/') ? PLANTECH_BASE_URL + link.uri : link.uri;
    const uri = link.uri;
    const isHttp = uri.startsWith('http');
    const isEmail = uri.startsWith('mailto:');
    const isPlanTech = uri.startsWith(PLANTECH_BASE_URL);

    if ((!isEmail && !isHttp) || isPlanTech) {
      internalLinks.push(link);
    } else if (isHttp && !isPlanTech) {
      externalLinks.push(link);
    }
  }

  return { internalLinks, externalLinks };
}

function groupLinksByUrl(links) {
  const grouped = new Map();

  for (const link of links) {
    if (!grouped.has(link.uri)) {
      grouped.set(link.uri, []);
    }
    grouped.get(link.uri).push(link);
  }

  return grouped;
}

async function checkExternalLink(url) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT);

  try {
    const res = await fetch(url, {
      method: 'GET',
      redirect: 'follow',
      signal: controller.signal,
      headers: {
        'User-Agent':
          'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36',
        Accept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        'Accept-Language': 'en-GB,en;q=0.9',
        Range: 'bytes=0-0',
      },
    });

    try {
      await res.arrayBuffer();
    } catch {
      // ignore buffer errors
    }

    clearTimeout(timeout);

    if (res.redirected) {
      return { valid: false, status: res.status, redirected: res.redirected, finalUrl: res.url };
    }

    if ([403, 406, 429].includes(res.status)) {
      return { valid: true, status: res.status, redirected: res.redirected };
    }

    return { valid: res.ok, status: res.status, redirected: res.redirected };
  } catch (e) {
    clearTimeout(timeout);
    return { valid: false, error: e.name };
  }
}

async function validateExternalLinks(groupedLinks) {
  const failedLinks = [];
  const failedExternalRows = [];
  const allExternalRows = []; // every instance, with its check result attached

  for (const [url, links] of groupedLinks.entries()) {
    if (!isValidUrl(url)) {
      // all instances of this invalid URL fail
      for (const link of links) {
        const result = { valid: false, reason: 'Invalid URL format' };

        failedLinks.push({
          id: link.entryId,
          uri: link.uri,
          reason: 'Invalid URL format',
        });

        failedExternalRows.push({
          entryId: link.entryId,
          internalName: link.internalName,
          uri: link.uri,
          status: '',
          redirected: '',
        });

        allExternalRows.push({ ...link, result });
      }
      continue;
    }
    const result = await checkExternalLink(url);

    for (const link of links) {
      allExternalRows.push({ ...link, result });

      if (!result.valid) {
        failedLinks.push({
          id: link.entryId,
          uri: link.uri,
          reason: result.error || `HTTP ${result.status}`,
        });

        failedExternalRows.push({
          entryId: link.entryId,
          internalName: link.internalName,
          uri: link.uri,
          status: result.status ?? '',
          redirected: result.redirected ?? false,
          finalUrl: result.finalUrl ?? '',
        });
      }
    }
  }

  return { failedLinks, failedExternalRows, allExternalRows };
}

function buildMarkdownReport(failedLinks, groupedExternal) {
  let md = `## Contentful Broken Link Check Summary\n`;
  // summary stats
  md += `\n### Summary\n`;
  md += `- Total external links checked: ${groupedExternal.size}\n`;
  md += `- Total link instances: ${Array.from(groupedExternal.values()).reduce((sum, arr) => sum + arr.length, 0)}\n`;
  md += `- Failed URLs: ${new Set(failedLinks.map((l) => l.uri)).size}\n`;
  md += `- Failed instances: ${failedLinks.length}\n`;
  md += `\n`;
  md += `Download the CSVs to view the results\n`;

  return md;
}

function csvEscape(value) {
  const s = String(value ?? '');
  // escape quotes by doubling them; wrap in quotes if contains commas/newlines/quotes
  if (/[",\n\r]/.test(s)) {
    return `"${s.replace(/"/g, '""')}"`;
  }
  return s;
}

function writeCsv(filePath, headers, rows) {
  const lines = [];
  lines.push(headers.map(csvEscape).join(','));
  for (const row of rows) {
    lines.push(headers.map((h) => csvEscape(row[h])).join(','));
  }
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, lines.join('\n'), 'utf8');
}

function exportLinkReportsToCsv(failedExternalRows, internalLinks) {
  // external links csv
  // columns -  entryId, link, statusCode, redirected
  const externalRows = failedExternalRows.map((r) => ({
    entryId: r.entryId,
    internalName: r.internalName,
    link: r.uri,
    statusCode: r.status,
    redirected: r.redirected,
    finalUrl: r.finalUrl,
  }));

  writeCsv(
    EXTERNAL_LINK_CSV_PATH,
    ['entryId', 'internalName', 'link', 'statusCode', 'redirected', 'finalUrl'],
    externalRows,
  );

  // internal links csv
  // columns - link, entryId
  const internalRows = internalLinks.map((l) => ({
    link: l.uri,
    entryId: l.entryId,
    internalName: l.internalName,
  }));

  writeCsv(INTERNAL_LINK_CSV_PATH, ['link', 'entryId', 'internalName'], internalRows);

  console.log(`Wrote CSV: ${EXTERNAL_LINK_CSV_PATH}`);
  console.log(`Wrote CSV: ${INTERNAL_LINK_CSV_PATH}`);
}

function writeHtmlReport(allExternalRows, internalLinks) {
  const html = generateHtmlReport({
    externalRows: allExternalRows,
    internalRows: internalLinks,
    spaceId: CONTENTFUL_SPACE_ID,
    environment: CONTENTFUL_ENVIRONMENT,
    runAt: new Date(),
  });

  fs.mkdirSync(path.dirname(HTML_REPORT_PATH), { recursive: true });
  fs.writeFileSync(HTML_REPORT_PATH, html, 'utf8');

  if (!CONTENTFUL_SPACE_ID) {
    console.warn(
      'CONTENTFUL_SPACE_ID is not set — "Open in Contentful" buttons will be disabled in the HTML report.',
    );
  }
  console.log(`Wrote HTML report: ${HTML_REPORT_PATH}`);
}

async function main() {
  const data = readLinksData();
  const { internalLinks, externalLinks } = categoriseLinks(data);
  const groupedExternal = groupLinksByUrl(externalLinks);

  console.log(
    `Checking ${groupedExternal.size} unique external URLs (${externalLinks.length} total instances)...`,
  );
  console.log();

  const { failedLinks, failedExternalRows, allExternalRows } =
    await validateExternalLinks(groupedExternal);

  const markdown = buildMarkdownReport(failedLinks, groupedExternal);

  const outPath = process.env.GITHUB_STEP_SUMMARY;
  if (outPath) {
    fs.appendFileSync(outPath, markdown);
    console.log('Wrote link summary to GITHUB_STEP_SUMMARY');
  } else {
    console.log(markdown);
  }

  exportLinkReportsToCsv(failedExternalRows, internalLinks);

  writeHtmlReport(allExternalRows, internalLinks);

  process.exit(0);
}

main();
