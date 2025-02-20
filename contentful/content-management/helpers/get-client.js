import contentfulManagement from "contentful-management";

const { createClient } = contentfulManagement;

/**
 * @typedef {Object} ClientAPI
 * @property {function(string, Object): Promise<CursorPaginatedCollection>} getEnvironmentTemplates - Gets all environment templates for an organization
 * @property {function(Object): Promise<EnvironmentTemplate>} getEnvironmentTemplate - Gets a specific environment template
 * @property {function(string, Object): Promise<EnvironmentTemplate>} createEnvironmentTemplate - Creates an environment template
 * @property {function(Object): Promise<Collection>} getSpaces - Gets all spaces
 * @property {function(string): Promise<Space>} getSpace - Gets a specific space
 * @property {function(Object, string): Promise<Space>} createSpace - Creates a new space
 * @property {function(string): Promise<Organization>} getOrganization - Gets a specific organization
 * @property {function(Object): Promise<Collection>} getOrganizations - Gets all organizations
 * @property {function(Object): Promise<User>} getCurrentUser - Gets the authenticated user
 * @property {function(Object): Promise<AppDefinition>} getAppDefinition - Gets an app definition
 * @property {function(Object): Promise<PersonalAccessToken>} createPersonalAccessToken - Creates a personal access token
 * @property {function(string): Promise<PersonalAccessToken>} getPersonalAccessToken - Gets a personal access token (deprecated)
 * @property {function(): Promise<Collection>} getPersonalAccessTokens - Gets all personal access tokens (deprecated)
 * @property {function(string): Promise<AccessToken>} getAccessToken - Gets a user's access token
 * @property {function(): Promise<Collection>} getAccessTokens - Gets all user access tokens
 * @property {function(string, Object): Promise<Collection>} getOrganizationAccessTokens - Gets all organization access tokens
 * @property {function(string, Object): Promise<Collection>} getOrganizationUsage - Gets organization usage metrics
 * @property {function(string, Object): Promise<Collection>} getSpaceUsage - Gets space usage metrics
 * @property {function(Object): Promise<any>} rawRequest - Makes a custom request to the API
 */

/**
 * Verifys that the environment specified in .env is a valid contentful environment
 * This is important because if it isn't, the management api fails silently
 * and falls back to using the master environment
 *
 * @param {ClientAPI} client
 */
export async function validateEnvironment(client) {
    const environments = await client.environment.getMany({
        spaceId: process.env.SPACE_ID,
    });
    const validNames = environments.items.map((env) => env.name);
    if (!validNames.includes(process.env.ENVIRONMENT)) {
        throw new Error(`Invalid Contentful environment`);
    }
}

/**
 * Creates client without validation
 * @returns {ClientAPI}
 */
export function getClient() {
    const client = createClient(
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

    return client;
}

/**
 * Gets Contentful Management ClientAPI and validates the environment from process.env
 * @returns {Promise<ClientAPI>}
 */
export async function getAndValidateClient() {
    const client = getClient();
    await validateEnvironment(client);
    return client;
}

export default getAndValidateClient;
