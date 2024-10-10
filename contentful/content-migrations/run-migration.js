const { runMigration } = require("contentful-migration");
const path = require("path");
const fs = require("fs");
require("dotenv/config");
async function main() {
    const fileName = process.argv[2];
    const filePath = fileName && path.join(__dirname, "migrations", fileName);

    if (!fs.existsSync(filePath)) {
        console.error("Invalid migration filename provided");
        return;
    }

    const options = {
        filePath: filePath,
        spaceId: process.env.SPACE_ID,
        environmentId: process.env.ENVIRONMENT,
        accessToken: process.env.MANAGEMENT_TOKEN,
    };

    await runMigration(options).catch(console.error);
}

main();
