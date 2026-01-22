#!/usr/bin/env node

import fs from "fs";
import path from "path";
import { loadAndSaveContentfulData } from "./contentful-data-loader.js";
import "dotenv/config";

const EXCLUDED_TAG_IDS = new Set(["e2e"]);
const CONTENTFUL_DATA_PATH = "./export/contentful-data.json";
const OUTPUT_PATH = "result/links.json";

function isPlainObject(v) {
  return v && typeof v === "object" && !Array.isArray(v);
}

function safeGet(obj, keys) {
  return keys.reduce((acc, k) => (acc?.[k] != null ? acc[k] : undefined), obj);
}

function getTagIds(entry) {
  const tags = entry?.metadata?.tags;
  if (!Array.isArray(tags)) return [];
  return tags.map((t) => t?.sys?.id).filter(Boolean);
}

function isEntry(obj) {
  return (
    isPlainObject(obj) &&
    isPlainObject(obj.sys) &&
    obj.sys.type === "Entry" &&
    typeof obj.sys.id === "string" &&
    isPlainObject(obj.fields)
  );
}

function isRichTextDocument(value) {
  return (
    isPlainObject(value) &&
    value.nodeType === "document" &&
    Array.isArray(value.content)
  );
}

function getNodeText(node) {
  if (!isPlainObject(node)) return "";
  if (node.nodeType === "text") return node.value || "";
  if (Array.isArray(node.content)) {
    return node.content.map(getNodeText).join("");
  }
  return "";
}

function walkRichText(node, callback, path = []) {
  if (!isPlainObject(node)) return;

  callback(node, path);

  if (Array.isArray(node.content)) {
    node.content.forEach((child, idx) => {
      walkRichText(child, callback, [...path, "content", idx]);
    });
  }
}

function extractLink(node) {
  const { nodeType } = node;

  if (nodeType === "hyperlink") {
    const uri = safeGet(node, ["data", "uri"]);
    if (!uri) return null;

    return {
      linkType: "hyperlink",
      uri,
      text: getNodeText(node).trim(),
    };
  }

  if (nodeType === "entry-hyperlink" || nodeType === "asset-hyperlink") {
    const targetSys = safeGet(node, ["data", "target", "sys"]);
    const targetId = targetSys?.id;
    if (!targetId) return null;

    return {
      linkType: nodeType,
      targetType: targetSys.linkType || (nodeType === "entry-hyperlink" ? "Entry" : "Asset"),
      targetId,
      text: getNodeText(node).trim(),
    };
  }

  return null;
}

function collectEntries(data) {
  const entries = [];
  const seen = new Set();

  // Check standard contentful export structure
  for (const key of ["entries", "contentTypes", "items"]) {
    const list = data?.[key];
    if (!Array.isArray(list)) continue;

    for (const item of list) {
      if (isEntry(item) && !seen.has(item.sys.id)) {
        entries.push(item);
        seen.add(item.sys.id);
      }
    }
  }

  // check all arrays at root
  for (const value of Object.values(data || {})) {
    if (!Array.isArray(value)) continue;

    for (const item of value) {
      if (isEntry(item) && !seen.has(item.sys.id)) {
        entries.push(item);
        seen.add(item.sys.id);
      }
    }
  }

  return entries;
}

function extractLinksFromEntry(entry) {
  const links = [];
  const entryId = safeGet(entry, ["sys", "id"]);
  const contentType = safeGet(entry, ["sys", "contentType", "sys", "id"]) || null;
  const fields = entry.fields || {};

  for (const [fieldId, fieldValue] of Object.entries(fields)) {
    // handle the localized fields
    if (isPlainObject(fieldValue) && !isRichTextDocument(fieldValue)) {
      for (const [locale, value] of Object.entries(fieldValue)) {
        if (!isRichTextDocument(value)) continue;

        walkRichText(value, (node, nodePath) => {
          const link = extractLink(node);
          if (link) {
            links.push({
              entryId,
              contentType,
              fieldId,
              locale,
              richTextPath: nodePath.join("."),
              ...link,
            });
          }
        });
      }
      continue;
    }

    // handle the non-localized rich text
    if (isRichTextDocument(fieldValue)) {
      walkRichText(fieldValue, (node, nodePath) => {
        const link = extractLink(node);
        if (link) {
          links.push({
            entryId,
            contentType,
            fieldId,
            locale: null,
            richTextPath: nodePath.join("."),
            ...link,
          });
        }
      });
    }
  }

  return links;
}

async function main() {
  await loadAndSaveContentfulData(process);

  if (!fs.existsSync(CONTENTFUL_DATA_PATH)) {
    console.error(`Contentful data not found at ${CONTENTFUL_DATA_PATH}`);
    process.exit(1);
  }

  let data;
  try {
    data = JSON.parse(fs.readFileSync(path.resolve(CONTENTFUL_DATA_PATH), "utf8"));
  } catch (e) {
    console.error("Failed to read/parse contentful data:", e.message);
    process.exit(1);
  }

  const entries = collectEntries(data).filter((entry) => {
    const tagIds = getTagIds(entry);
    return !tagIds.some((id) => EXCLUDED_TAG_IDS.has(id));
  });

  const links = entries.flatMap(extractLinksFromEntry);

  fs.mkdirSync(path.dirname(OUTPUT_PATH), { recursive: true });
  fs.writeFileSync(OUTPUT_PATH, JSON.stringify(links, null, 2), "utf8");

  console.log(`Extracted ${links.length} links from ${entries.length} entries`);
}

main();