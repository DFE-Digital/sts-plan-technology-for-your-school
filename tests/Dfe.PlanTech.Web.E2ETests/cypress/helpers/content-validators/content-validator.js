import ValidateHeader from "./header-validator";
import { ValidateButtonWithLink } from "./button-validator";
import ValidateRichTextContent from "./rich-text-content-validator";

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
    //ButtonWithReference

    //Category

    //Section??

    //Question?Answer?

    //Recommendation Page
    default: {
      console.log(content.sys.contentType.sys.id, content);
      break;
    }
  }
}

export default ValidateContent;
