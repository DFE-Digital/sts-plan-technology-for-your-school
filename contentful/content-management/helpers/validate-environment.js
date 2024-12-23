/**
 * Verifies that the environment specified in .env is a valid contentful environment
 * This is important because if it isn't, the management api fails silently
 * and falls back to using the master environment
 *
 * @param {ClientAPI | null} client
 */
module.exports = async function validateEnvironment(client) {
    const environments = await client.environment.getMany({
        spaceId: process.env.SPACE_ID,
    });
    const validNames = environments.items.map((env) => env.name);
    if (!validNames.includes(process.env.ENVIRONMENT)) {
        throw new Error(`Invalid Contentful environment`);
    }
}
