import {
  ValidateButtonWithLink,
  ValidateButtonWithEntryReference,
} from "./button-validator";
import ValidateCategory from "./category-validator";
import ValidateHeader from "./header-validator";
import ValidateInsetTextContent from "./inset-text-validator";
import ValidateNavigationLink from "./nav-link-validator";
import ValidateRichTextContent from "./rich-text-content-validator";
import ValidateWarningComponent from "./warning-validator";

function ValidateContent(content) {
  switch (content.sys.contentType.sys.id) {
    case "header": {
      ValidateHeader(content);
      break;
    }
    case "buttonWithLink": {
      ValidateButtonWithLink(content);
      break;
    }
    case "textBody": {
      ValidateRichTextContent(content.fields.richText);
      break;
    }
    case "category": {
      ValidateCategory(content);
      break;
    }
    case "warningComponent": {
      ValidateWarningComponent(content);
      break;
    }
    case "buttonWithEntryReference": {
      ValidateButtonWithEntryReference(content);
      break;
    }
    case "insetText": {
      ValidateInsetTextContent(content);
      break;
    }
    case "navigationLink": {
      ValidateNavigationLink(content);
      break;
    }
    default: {
      console.log(content.sys.contentType.sys.id, content);
      break;
    }
  }
}

export default ValidateContent;
