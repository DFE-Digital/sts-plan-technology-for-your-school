import ValidateRichTextContent from "./rich-text-content-validator.js";

export default function ValidateWarningComponent({ fields, sys }) {
  ValidateRichTextContent(fields.text.fields.richText);
}
