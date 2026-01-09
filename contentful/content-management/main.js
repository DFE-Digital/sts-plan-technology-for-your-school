import path from "path";
import { fileURLToPath, pathToFileURL } from "url";
import { existsSync } from "fs";
import "dotenv/config";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export async function main() {
  const fileName = process.argv[2];

  if (!fileName) {
    console.error("Usage: npm run crud-operation -- YYYYMMDD-HHMM-description-of-crud-operation.js");
    process.exit(1);
  }

  const normalizedName = fileName.endsWith(".js") ? fileName : `${fileName}.js`;
  const filePath = path.join(__dirname, "changes", normalizedName);

  if (!existsSync(filePath)) {
    console.error('Invalid filename provided');
    return;
  }

  const fileUrl = pathToFileURL(filePath)

  const { default: changeFunction } = await import(fileUrl.href);
  await changeFunction();
}

if (import.meta.url.startsWith('file:') && process.argv[1] === fileURLToPath(import.meta.url)) {
  main();
}
