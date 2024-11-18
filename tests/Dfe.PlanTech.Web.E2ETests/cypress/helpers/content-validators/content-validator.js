import {
  ValidateButtonWithLink,
  ValidateButtonWithEntryReference,
} from "./button-validator.js";
import ValidateCategory from "./category-validator.js";
import ValidateHeader from "./header-validator.js";
import ValidateInsetTextContent from "./inset-text-validator.js";
import ValidateNavigationLink from "./nav-link-validator.js";
import ValidateRichTextContent from "./rich-text-content-validator.js";
import ValidateWarningComponent from "./warning-validator.js";
import ValidateNotificationBanner from "./notification-banner-validator.js";

export const ValidateContent = (content) => {
    if (!content.sys || !content.fields) {
        console.log(`Content is missing sys or fields. Skipping`, content);
    }
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
        case "notificationBanner": {
            ValidateNotificationBanner(content);
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
};
