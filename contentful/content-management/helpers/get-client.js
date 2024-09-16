const contentful = require("contentful-management");

module.exports = function getClient () {
    return contentful.createClient(
        {
            accessToken: process.env.MANAGEMENT_TOKEN,
        },
        {
            type: 'plain',
            defaults: {
                spaceId: process.env.SPACE_ID,
                environmentId: process.env.ENVIRONMENT
            }
        }
    )
}
