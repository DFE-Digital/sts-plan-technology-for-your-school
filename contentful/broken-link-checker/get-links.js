#!/usr/bin/env node

import fs from "fs";
import path from "path";
import { loadAndSaveContentfulData } from "./contentful-data-loader.js";
import "dotenv/config";

const EXCLUDED_TAG_IDS = new Set(["e2e"]);

const filePath = "./export/contentful-data.json";
const outputFile = "result/links.json";

const isPlainObject = (v) => v && typeof v === "object" && !Array.isArray(v);

const safeGet = (obj, keys) =>
  keys.reduce((acc, k) => (acc && acc[k] != null ? acc[k] : undefined), obj);

const getTagIds = (entry) => {
  const tags = entry?.metadata?.tags;
  if (!Array.isArray(tags)) return [];
  return tags.map((t) => t?.sys?.id).filter(Boolean);
};

const isEntryLike = (obj) =>
  isPlainObject(obj) &&
  isPlainObject(obj.sys) &&
  obj.sys.type === "Entry" &&
  typeof obj.sys.id === "string" &&
  isPlainObject(obj.fields);

const isRichTextDocument = (value) =>
  isPlainObject(value) &&
  value.nodeType === "document" &&
  Array.isArray(value.content);

const nodeText = (node) => {
  if (!isPlainObject(node)) return "";
  if (node.nodeType === "text") return node.value || "";
  if (Array.isArray(node.content)) return node.content.map(nodeText).join("");
  return "";
};

const walkRichText = (node, onNode, nodePath = []) => {
  if (!isPlainObject(node)) return;

  onNode(node, nodePath);

  const content = node.content;
  if (!Array.isArray(content)) return;

  content.forEach((child, idx) => {
    walkRichText(child, onNode, nodePath.concat(["content", idx]));
  });
};

const extractLinkFromNode = (node) => {
  const nt = node.nodeType;

  if (nt === "hyperlink") {
    const uri = safeGet(node, ["data", "uri"]);
    if (!uri) return null;

    return {
      linkType: "hyperlink",
      uri,
      text: nodeText(node).trim(),
    };
  }

  if (nt === "entry-hyperlink" || nt === "asset-hyperlink") {
    const targetSys = safeGet(node, ["data", "target", "sys"]);
    const targetId = targetSys?.id;
    if (!targetId) return null;

    return {
      linkType: nt,
      targetType: targetSys.linkType || (nt === "entry-hyperlink" ? "Entry" : "Asset"),
      targetId,
      text: nodeText(node).trim(),
    };
  }

  return null;
};

const collectEntries = (root) => {
  const found = [];

  for (const key of ["entries", "contentTypes", "items"]) {
    const list = root?.[key];
    if (!Array.isArray(list)) continue;

    for (const item of list) {
      if (isEntryLike(item)) found.push(item);
    }
  }

  for (const value of Object.values(root || {})) {
    if (!Array.isArray(value)) continue;

    for (const item of value) {
      if (isEntryLike(item)) found.push(item);
    }
  }

  const seen = new Set();
  return found.filter((e) => {
    if (seen.has(e.sys.id)) return false;
    seen.add(e.sys.id);
    return true;
  });
};


// get the contentful data
await loadAndSaveContentfulData(process);

if (!filePath) {
  console.error("Contentful JSON does not exist");
  process.exit(1);
}

//read the file
let data;
try {
  data = JSON.parse(fs.readFileSync(path.resolve(filePath), "utf8"));
} catch (e) {
  console.error("Failed to read/parse JSON:", e.message);
  process.exit(1);
}

const entries = collectEntries(data).filter((entry) => {
  const tagIds = getTagIds(entry);
  return !tagIds.some((id) => EXCLUDED_TAG_IDS.has(id));
});

const results = [];

for (const entry of entries) {
  const entryId = safeGet(entry, ["sys", "id"]);
  const contentType = safeGet(entry, ["sys", "contentType", "sys", "id"]) || null;

  const fields = entry.fields || {};
  for (const fieldId of Object.keys(fields)) {
    const localizedOrValue = fields[fieldId];

    if (isPlainObject(localizedOrValue)) {
      for (const [locale, value] of Object.entries(localizedOrValue)) {
        if (!isRichTextDocument(value)) continue;

        walkRichText(value, (node, nodePath) => {
          const link = extractLinkFromNode(node);
          if (!link) return;

          results.push({
            entryId,
            contentType,
            fieldId,
            locale,
            richTextPath: nodePath.join("."),
            ...link,
          });
        });
      }
      continue;
    }

    if (!isRichTextDocument(localizedOrValue)) continue;

    walkRichText(localizedOrValue, (node, nodePath) => {
      const link = extractLinkFromNode(node);
      if (!link) return;

      results.push({
        entryId,
        contentType,
        fieldId,
        locale: null,
        richTextPath: nodePath.join("."),
        ...link,
      });
    });
  }
}

fs.mkdirSync(path.dirname(outputFile), { recursive: true });
fs.writeFileSync(outputFile, JSON.stringify(results, null, 2), "utf8");
