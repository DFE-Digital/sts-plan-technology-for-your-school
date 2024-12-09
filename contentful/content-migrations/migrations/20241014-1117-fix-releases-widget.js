const Migration = require("contentful-migration");
const { runForEachContentType } = require("../helpers/content-types");

/**
 *
 * @param {Migration} migration
 */
module.exports = function (migration) {
  runForEachContentType((contentTypeName) => updateContentType(migration, contentTypeName));
};

/**
 *
 * @param {Migration} migration
 * @param {string} contentType
 */
function updateContentType(migration, contentType) {
  var contentType = migration.editContentType(contentType);

  contentType.removeSidebarWidget("builtin", "releases-widget");

  //6RKxbgPghdY4llDpwCFvgR was pulled from the Contentful export - _should_ be the "workflows widget"
  //No idea why it shows as this, but it's there, and the releases widget should be first
  contentType.addSidebarWidget("sidebar-builtin", "releases-widget", undefined, "6RKxbgPghdY4llDpwCFvgR");
}
