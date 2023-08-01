import { create, run } from "./migrator.js";
import getArguments from "./get-arguments.js";

function createAndExecuteMigrations() {
    const args = getArguments();
    const verbose = args.verbose;

    console.log(`Attempting to migrate from ${args.se} to ${args.te}`);
    
    const result = create({ source: args.se, target: args.te, verbose });

    if (!result.continue) {
        return;
    }

    run({
        environment: args.te,
        spaceId: args["space-id"],
        migrationFile: result.path,
        verbose
    });

    return result.path;
}

const migrationsPath = createAndExecuteMigrations();

if (migrationsPath) {
    console.log(`Applied migrations from ${migrationsPath}`);
}