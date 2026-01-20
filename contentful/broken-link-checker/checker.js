import fs from "fs";
import path from "path";
import "dotenv/config";

const PLANTECH_BASE_URL = "https://www.plan-technology-for-your-school.education.gov.uk";
const LINKS_FILE_PATH = path.resolve("./result/links.json");
const REQUEST_TIMEOUT = 15000;

const CSV_REPORT_DIR = path.resolve("./report");
const EXTERNAL_LINK_CSV_PATH = path.join(CSV_REPORT_DIR, "failed-external-links.csv");
const INTERNAL_LINK_CSV_PATH = path.join(CSV_REPORT_DIR, "internal-links-to-check.csv");

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
    return JSON.parse(fs.readFileSync(LINKS_FILE_PATH, "utf8"));
  } catch (e) {
    console.error("Failed to read/parse links file:", e.message);
    process.exit(1);
  }
}

function categoriseLinks(data) {
  const internalLinks = [];
  const externalLinks = [];

  for (const link of data) {
    const uri = link.uri;
    const isHttp = uri.startsWith("http");
    const isEmail = uri.startsWith("mailto:");
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
      method: "GET",
      redirect: "follow",
      signal: controller.signal,
      headers: {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36",
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Language": "en-GB,en;q=0.9",
        "Range": "bytes=0-0",
      },
    });

    try {
      await res.arrayBuffer();
    } catch {
      // ignore buffer errors
    }

    clearTimeout(timeout);

    if (res.redirected) {
        return { valid: false, status: res.status, redirected: res.redirected };
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

  for (const [url, links] of groupedLinks.entries()) {
    if (!isValidUrl(url)) {
      // all instances of this invalid URL fail
      for (const link of links) {
        failedLinks.push({
          id: link.entryId,
          uri: link.uri,
          reason: "Invalid URL format",
        });

        failedExternalRows.push({
          entryId: link.entryId,
          uri: link.uri,
          status: "",
          redirected: "",
        });
      }
      continue;
    }

    const result = await checkExternalLink(url);

    if (!result.valid) {
      for (const link of links) {
        failedLinks.push({
          id: link.entryId,
          uri: link.uri,
          reason: result.error || `HTTP ${result.status}`,
        });

        failedExternalRows.push({
          entryId: link.entryId,
          uri: link.uri,
          status: result.status ?? "",
          redirected: result.redirected ?? false,
        });
      }
    }
  }

  return { failedLinks, failedExternalRows };
}

function buildMarkdownReport(failedLinks, internalLinks, groupedExternal) {
  let md = `## Contentful Broken Link Check Summary\n\n`;

  // failed external links
  md += `### ❌ Failed External Links (${failedLinks.length} instances across ${new Set(failedLinks.map(l => l.uri)).size} unique URLs)\n`;
  if (failedLinks.length === 0) {
    md += `_None_ ✅\n`;
  } else {

    // group by the url
    const failedByUrl = new Map();
    for (const link of failedLinks) {
      if (!failedByUrl.has(link.uri)) {
        failedByUrl.set(link.uri, []);
      }
      failedByUrl.get(link.uri).push(link);
    }

    for (const [url, instances] of failedByUrl.entries()) {
      md += `\n**${url}** (${instances.length} instance${instances.length > 1 ? 's' : ''})\n`;
      for (const link of instances) {
        md += `  - Entry: ${link.id}${link.reason ? ` - ${link.reason}` : ''}\n`;
      }
    }
  }

  // internal links to check
  md += `\n### Internal Links to Check (${internalLinks.length})\n`;
  if (internalLinks.length === 0) {
    md += `_None_ ✅\n`;
  } else {
    internalLinks.forEach(link => {
      md += `- ${link.uri} - [${link.entryId}]\n`;
    });
  }

  // summary stats
  md += `\n### Summary\n`;
  md += `- Total external links checked: ${groupedExternal.size}\n`;
  md += `- Total link instances: ${Array.from(groupedExternal.values()).reduce((sum, arr) => sum + arr.length, 0)}\n`;
  md += `- Failed URLs: ${new Set(failedLinks.map(l => l.uri)).size}\n`;
  md += `- Failed instances: ${failedLinks.length}\n`;

  return md;
}

function csvEscape(value) {
  const s = String(value ?? "");
  // escape quotes by doubling them; wrap in quotes if contains commas/newlines/quotes
  if (/[",\n\r]/.test(s)) {
    return `"${s.replace(/"/g, '""')}"`;
  }
  return s;
}

function writeCsv(filePath, headers, rows) {
  const lines = [];
  lines.push(headers.map(csvEscape).join(","));
  for (const row of rows) {
    lines.push(headers.map((h) => csvEscape(row[h])).join(","));
  }
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, lines.join("\n"), "utf8");
}

function exportLinkReportsToCsv(failedExternalRows, internalLinks) {
  // external links csv
  // columns -  entryId, link, statusCode, redirected
  const externalRows = failedExternalRows.map((r) => ({
    entryId: r.entryId,
    link: r.uri,
    statusCode: r.status,
    redirected: r.redirected,
  }));

  writeCsv(
    EXTERNAL_LINK_CSV_PATH,
    ["entryId", "link", "statusCode", "redirected"],
    externalRows
  );

  // internal links csv
  // columns - link, entryId
  const internalRows = internalLinks.map((l) => ({
    link: l.uri,
    entryId: l.entryId,
  }));

  writeCsv(
    INTERNAL_LINK_CSV_PATH,
    ["link", "entryId"],
    internalRows
  );

  console.log(`Wrote CSV: ${EXTERNAL_LINK_CSV_PATH}`);
  console.log(`Wrote CSV: ${INTERNAL_LINK_CSV_PATH}`);
}


async function main() {
  const data = readLinksData();
  const { internalLinks, externalLinks } = categoriseLinks(data);
  const groupedExternal = groupLinksByUrl(externalLinks);

  console.log(`Checking ${groupedExternal.size} unique external URLs (${externalLinks.length} total instances)...`);

  const { failedLinks, failedExternalRows } = await validateExternalLinks(groupedExternal);

  const markdown = buildMarkdownReport(failedLinks, internalLinks, groupedExternal);

  const outPath = process.env.GITHUB_STEP_SUMMARY;
  if (outPath) {
    fs.appendFileSync(outPath, markdown);
    console.log("Wrote link summary to GITHUB_STEP_SUMMARY");
  } else {
    console.log(markdown);
  }

    exportLinkReportsToCsv(failedExternalRows, internalLinks);


  process.exit(0);
}

main();