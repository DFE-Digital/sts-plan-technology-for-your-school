import { execSync } from "child_process";
import { readFileSync } from "fs";

export function run({ environment, spaceId, migrationFile, verbose }) {
    const runMigrationCommand = `contentful space migration --space-id ${spaceId} --environment-id ${environment} -y "${migrationFile}"`;

    if (verbose) {
        console.log("Executing command " + runMigrationCommand);
    }

    try {
        const mergeResult = execSync(runMigrationCommand);

        if (verbose) {
            console.log(mergeResult.toString());
        }

        const isSuccess = mergeResult.indexOf("Migration successful") > -1;

        if (!isSuccess) {
            throw mergeResult;
        }
    }
    catch (error) {
        console.log(`Error running migration ${migrationFile}`, error.stderr?.toString() ?? error.stdout?.toString() ?? error);
    }

}

export function create({ source, target, verbose }) {
    const mergeCommand = `contentful merge export --te ${target} --se ${source}`;

    if (verbose) {
        console.log("Executing command " + mergeCommand);
    }

    try {
        const mergeResult = execSync(mergeCommand);

        if (verbose) {
            console.log(mergeResult.toString());
        }

        const exportResultLine = mergeResult.toString().split("\n")
            .find(line => line.indexOf("Migration exported to") > -1);

        if (!exportResultLine) {
            throw `Could not find export result`;
        }

        const path = exportResultLine.split("Migration exported to ")[1].replace(".js.", ".js");
        const results = readFileSync(path).toString();

        if (verbose) {
            console.log(results);
        }

        const lineCount = (results.match(/\n/g) || []).length;

        const result = {
            success: true,
            path,
            results,
            continue: lineCount > 3
        }

        if (!result.continue) {
            console.log("No migrations to apply");
        }

        return result;
    }
    catch (error) {
        console.log(`Error creating migrations for ${source} to ${target}`, error.stderr?.toString() ?? error.stdout?.toString() ?? error);
    }
}
