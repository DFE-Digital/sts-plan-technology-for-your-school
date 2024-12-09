const contentful = require("contentful-management");

/**
 * Verifys that the environment specified in .env is a valid contentful environment
 * This is important because if it isn't, the management api fails silently
 * and falls back to using the master environment
 *
 * @param {ClientAPI} client
 */
async function validateEnvironment(client) {
    const environments = await client.environment.getMany({
        spaceId: process.env.SPACE_ID,
    });
    const validNames = environments.items.map((env) => env.name);
    if (!validNames.includes(process.env.ENVIRONMENT)) {
        throw new Error(`Invalid Contentful environment`);
    }
}

module.exports = async function getClient() {
    const client = contentful.createClient(
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
    await validateEnvironment(client);
    return client;
};
