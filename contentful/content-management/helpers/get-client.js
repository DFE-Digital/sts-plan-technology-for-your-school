const contentful = require("contentful-management");
const validateEnvironment = require("./validate-environment");


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
