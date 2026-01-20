import fs from "fs";
import path from "path";
import "dotenv/config";

function isValidUrlFormat(uri) {
  try {
    new URL(uri);
    return true;
  } catch {
    return false;
  }
}

const filePath = path.resolve("./result/links.json");

if (!fs.existsSync(filePath)) {
  console.error(`Contentful JSON does not exist at ${filePath}`);
  process.exit(1);
}

// get the data
let data;
try {
  data = JSON.parse(fs.readFileSync(filePath, "utf8"));
} catch (e) {
  console.error("Failed to read/parse JSON:", e.message);
  process.exit(1);
}

// Internal links to check (ignore external http + include any http plantech links)
const internalLinks = data.filter(d => {
  const uri = d.uri;
  const isHttp = uri.startsWith("http");
  const isEmail = uri.startsWith("mailto:");
  const isPlanTech = uri.startsWith("https://www.plan-technology-for-your-school.education.gov.uk");

  return (!isEmail && !isHttp) || isPlanTech;
});

// all the links that aren't plan-tech
const nonPlanTechHttpLinks = data.filter(d => d.uri.startsWith("http") && !d.uri.startsWith("https://www.plan-technology-for-your-school.education.gov.uk"));

// remove any duplicates
const uniqueExternalLinks = [...new Map(nonPlanTechHttpLinks.map(item => [item.uri, item])).values()];

async function checkExternalLink(url) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 15000);

  try {
    const res = await fetch(url, {
      method: "GET",
      redirect: "follow",
      signal: controller.signal,
      headers: {
        "User-Agent":
          "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36",
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Accept-Language": "en-GB,en;q=0.9",
        "Range": "bytes=0-0",
      },
    });

    // clear the buffer
    try {
      await res.arrayBuffer();
    } catch {
      // ignore buffer errors
    }

    clearTimeout(timeout);

    if (res.ok) return { valid: true };

    return { valid: false, status: res.status };
  } catch (e) {
    clearTimeout(timeout);
    return { valid: false, error: e.name };
  }
}

let failedExternalLinks = [];

async function run() {
  for (const ex of uniqueExternalLinks) {
    const isValidUrl = isValidUrlFormat(ex.uri);
    if (!isValidUrl) {
      failedExternalLinks.push({ id: ex.entryId, uri: ex.uri });
      continue;
    }

    const result = await checkExternalLink(ex.uri);

    if (!result.valid) {
      failedExternalLinks.push({ id: ex.entryId, uri: ex.uri });
    }
  }
}

await run();

// github summary output
const outPath = process.env.GITHUB_STEP_SUMMARY;

// build the markdown
let md = `## Contentful Broken Link Check Summary\n\n`;

// Failed external links
md += `### ❌ Failed External Links (${failedExternalLinks.length})\n`;
if (failedExternalLinks.length === 0) {
  md += `_None_ ✅\n`;
} else {
  failedExternalLinks.forEach(link => {
    md += `- ❌ (${link.uri}) - [${link.id}]\n`;
  });
}

// internal links to check
md += `\n### Internal Links to Check (${internalLinks.length})\n`;
if (internalLinks.length === 0) {
  md += `_None_ ✅\n`;
} else {
  internalLinks.forEach(link => {
    md += `- (${link.uri}) - [${link.entryId}]\n`;
  });
}

md += `\n`;

// output to github
if (outPath) {
  fs.appendFileSync(outPath, md);
  console.log("Wrote link summary to GITHUB_STEP_SUMMARY");
} else {
  console.log(md);
}

// Explicitly exit to close any lingering connections
process.exit(0);