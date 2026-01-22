import ValidateRichTextContent from './rich-text-content-validator.js';

export default function ValidateWarningComponent({ fields }) {
  ValidateRichTextContent(fields.text.fields.richText);
}
