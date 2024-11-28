const freeTextFields = {
    CSBodyText: ["title", "subTitle"],
    ContentSupportPage: ["Slug"],
    page: ["slug"],
    section: ["name"],
    AccordionSection: ["Title", "SummaryLine"],
    Attachment: ["Title"],
    recommendationIntro: ["slug"],
    navigationLink: ["displayText", "href"],
    answer: ["text"],
    question: ["helpText", "slug"],
    buttonWithLink: ["href"],
    button: ["value"],
    componentDropDown: ["title"],
    title: ["text"],
    header: ["text"],
    csLink: ["url", "linkText"],
};
const urlFieldTypes = ["href", "url", "slug"];
const regexNoLeadingOrTrailingWhitespace = "^(?!\\s).*(?<!\\s)$";
const regexNoWhitespace = "[^\\s]+$";
const regexValidQuestion = "^(?!\\s).*\?$";

/**
 *
 * @param {Migration} migration
 */
module.exports = function (migration) {
    Object.entries(freeTextFields).forEach(([contentTypeId, textFields]) => {
        textFields.forEach((fieldId) => {
            addValidationToTextField(migration, contentTypeId, fieldId);
            stripWhitespaceFromTextField(migration, contentTypeId, fieldId);
        });
    });

    // Question text is a bit different, and should terminate in a question mark
    let contentType = migration.editContentType("question");
    let field = contentType.editField("text");
    field.validations([
        {
            regexp: { pattern: regexValidQuestion },
            message: "Questions must end in a ? and not begin with whitespace.",
        },
    ]);
};

/**
 * Add validation to text fields. Slugs cannot contain any whitespace, other fields it cannot be leading or trailing
 * @param {Migration} migration
 * @param {string} contentType
 * @param {string} fieldId
 */
function addValidationToTextField(migration, contentTypeName, fieldId) {
    let contentType = migration.editContentType(contentTypeName);
    let field = contentType.editField(fieldId);

    console.log(`Adding validation to ${contentTypeName}: ${fieldId}`);

    if (urlFieldTypes.includes(fieldId)) {
        field.validations([
            {
                regexp: { pattern: regexNoWhitespace },
                message: "Urls cannot contain whitespace.",
            },
        ]);
    } else {
        field.validations([
            {
                regexp: { pattern: regexNoLeadingOrTrailingWhitespace },
                message: "Cannot have leading or trailing whitespace.",
            },
        ]);
    }
}

/**
 * Strip leading or trailing whitespace from any text field or slug
 * @param {Migration} migration
 * @param {string} contentType
 * @param {string} fieldId
 */
function stripWhitespaceFromTextField(migration, contentType, fieldId) {
    console.log(`Stripping whitespace from ${contentType}: ${fieldId}`);

    migration.transformEntries({
        contentType: contentType,
        from: [fieldId],
        to: [fieldId],
        transformEntryForLocale: (fromFields, currentLocale) => {
            let value = fromFields[fieldId]?.[currentLocale];
            return {
                [fieldId]: value?.trim() ?? value,
            };
        },
    });
}
