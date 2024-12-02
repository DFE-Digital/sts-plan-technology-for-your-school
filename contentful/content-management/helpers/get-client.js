const contentful = require("contentful-management");
const validateEnvironment = require("./validate-environment");

module.exports = function getClient() {
    validateEnvironment();
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
        },
    );
};
