import path from "path";
import { fileURLToPath } from "url";
import { existsSync } from "fs";
import "dotenv/config";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export async function main() {
    const fileName = process.argv[2];
    const filePath = fileName && path.join(__dirname, "changes", fileName);

    if (!existsSync(filePath)) {
        console.error("Invalid filename provided");
        return;
    }

    const { default: changeFunction } = await import(filePath);
    await changeFunction();
}

if (
    import.meta.url.startsWith("file:") &&
    process.argv[1] === fileURLToPath(import.meta.url)
) {
    main();
}
