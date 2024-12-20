const contentful = require("contentful-management");

function createClient() {
    return contentful.createClient(
        {
            accessToken: process.env.MANAGEMENT_TOKEN,
        },
        {
            type: "plain",
            defaults: {
                spaceId: process.env.SPACE_ID,
                environmentId: process.env.ENVIRONMENT,
            },
        }
    );
}

/**
 * Verifies that the environment specified in .env is a valid contentful environment
 * This is important because if it isn't, the management api fails silently
 * and falls back to using the master environment
 *
 * @param {ClientAPI | null} client
 */
module.exports = async function validateEnvironment(client = null) {
    if (!client) {
        client = createClient();
    }
    const environments = await client.environment.getMany({
        spaceId: process.env.SPACE_ID,
    });
    const validNames = environments.items.map((env) => env.name);
    if (!validNames.includes(process.env.ENVIRONMENT)) {
        throw new Error(`Invalid Contentful environment`);
    }
}
