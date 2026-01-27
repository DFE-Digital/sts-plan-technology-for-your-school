const freeTextFields = {
  CSBodyText: ['title', 'subtitle'],
  ContentSupportPage: ['Slug'],
  page: ['slug'],
  section: ['name'],
  AccordionSection: ['Title', 'SummaryLine'],
  Attachment: ['Title'],
  recommendationIntro: ['slug'],
  navigationLink: ['displayText', 'href'],
  answer: ['text'],
  question: ['helpText', 'slug'],
  buttonWithLink: ['href'],
  button: ['value'],
  componentDropDown: ['title'],
  title: ['text'],
  header: ['text'],
  csLink: ['url', 'linkText'],
};
const urlFieldTypes = ['href', 'url', 'slug'];

// Contentful does not like negative lookahead so regexes have to be marginally more complex
const regexNoLeadingOrTrailingWhitespace = '^[^\\s]+.*[^\\s]+$|^[^\\s]+$';
const regexNoWhitespace = '^[^\\s]+$';
const regexValidQuestion = '^[^\\s]+.*\\?$';

/**
 *
 * @param {Migration} migration
 */
module.exports = function (migration) {
  Object.entries(freeTextFields).forEach(([contentTypeId, textFields]) => {
    textFields.forEach((fieldId) => {
      stripWhitespaceFromTextField(migration, contentTypeId, fieldId);
      addValidationToTextField(migration, contentTypeId, fieldId);
    });
  });

  // Question text is a bit different, and should terminate in a question mark
  stripWhitespaceFromTextField(migration, 'question', 'text');
  let contentType = migration.editContentType('question');
  let field = contentType.editField('text');
  field.validations([
    {
      regexp: { pattern: regexValidQuestion },
      message: 'Questions must end in a ? and not begin with whitespace.',
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

  if (urlFieldTypes.includes(fieldId.toLowerCase())) {
    field.validations([
      {
        regexp: { pattern: regexNoWhitespace, flags: null },
        message: 'Urls cannot contain whitespace.',
      },
    ]);
  } else {
    field.validations([
      {
        regexp: { pattern: regexNoLeadingOrTrailingWhitespace, flags: null },
        message: 'Cannot have leading or trailing whitespace.',
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
