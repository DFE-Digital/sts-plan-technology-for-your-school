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
    var page = migration.editContentType(contentType);

    for (const reference of references) {
        page.changeFieldControl(reference, "builtin", "entryLinksEditor", { bulkEditing: true });
    }

    page.addSidebarWidget("builtin", "publication-widget", undefined, "releases-widget");
}