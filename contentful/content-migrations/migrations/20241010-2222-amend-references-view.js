const Migration = require("contentful-migration");

const contentTypeConfigurations = {
    "button": [],
    "buttonWithEntryReference": [],
    "buttonWithLink": [],
    "category": ["content", "sections"],
    "componentDropDown": [],
    "csLink": [],
    "header": [],
    "insetText": [],
    "navigationLink": [],
    "page": ["content", "beforeTitleContent"],
    "question": ["answers"],
    "recommendationChunk": ["answers", "content"],
    "recommendationIntro": ["content"],
    "recommendationSection": ["answers", "chunks"],
    "section": ["questions"],
    "subtopicRecommendation": ["intros"],
    "textBody": [],
    "title": [],
    "warningComponent": [],
};

/**
 * 
 * @param {Migration} migration 
 */
module.exports = function (migration) {
    for (const [contentType, references] of Object.entries(contentTypeConfigurations)) {
        updateContentType(migration, contentType, references);
    }
};

/**
 * 
 * @param {Migration} migration 
 * @param {string} contentType 
 * @param {string[]} references 
 */
function updateContentType(migration, contentType, references) {
    var contentType = migration.editContentType(contentType);

    for (const reference of references) {
        contentType.changeFieldControl(reference, "builtin", "entryLinksEditor", { bulkEditing: true });
    }

    contentType.addSidebarWidget("builtin", "releases-widget", undefined, "publication-widget");
}