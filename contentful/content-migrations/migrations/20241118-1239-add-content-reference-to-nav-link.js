const Migration = require("contentful-migration");

/**
 * 
 * @param {Migration} migration 
 */
module.exports = function (migration) {
  const fieldName = "Content to link to";
  const navLinkContentType = migration.editContentType("navigationLink");

  navLinkContentType.createField("contentToLinkTo", {
    "name": fieldName,
    "type": "Link",
    "linkType": "Entry",
    "validations": [{
      "linkContentType": ["page", "ContentSupportPage"]
    }],
  });

  navLinkContentType.moveField("contentToLinkTo").beforeField("openInNewTab");

  navLinkContentType.changeFieldControl("contentToLinkTo", "builtin", "entryCardEditor", {
    "showLinkEntityAction": true,
    "showCreateEntityAction": false,
    "helpText": "The page or content support page that this navigation link should link to. This will take precedence over the HREF field."
  });

  navLinkContentType.changeFieldControl("href", "builtin", "urlEditor", {
    "helpText": `URL to link to. Only use this for external links - if you want to link to internal content then please use the "${fieldName}" to field.`
  });

  navLinkContentType.editField("href", {
    required: false
  });
};
