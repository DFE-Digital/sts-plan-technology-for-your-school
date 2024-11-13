import ValidateRichTextContent from "./rich-text-content-validator.js";

export default function ValidateNotificationBanner({ fields }) {
  ValidateRichTextContent(fields.text.fields.richText);
}
