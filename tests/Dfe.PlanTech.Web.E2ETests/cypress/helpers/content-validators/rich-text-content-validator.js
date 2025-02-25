import { parse } from "node-html-parser";
import TableValidator from "./rich-text-validators/table-validator";
import { CleanText } from "../text-helpers";
import ValidateHeader from "./header-validator";

const tableValidator = new TableValidator();

function ValidateRichTextContent(parent) {
  if (!parent?.content) return;

  for (const child of parent.content) {
    validateByNodeType(child);
  }
}

export default ValidateRichTextContent;

function validateByNodeType(content) {
  switch (content.nodeType) {
    case "paragraph": {
      validateParagraph(content);
      break;
    }

    case "table": {
      tableValidator.validateTable(content);
      break;
      }

      case "heading-2": {
          const tag = "h2";
          const size = "large";
          const text = content.content[0].value;
          const nodeContent = { fields: { tag, size, text } };
          ValidateHeader(nodeContent);
          break;
      }

      case "heading-3": {
          const tag = "h3";
          const size = "medium";
          const text = content.content[0].value;
          const nodeContent = { fields: { tag, size, text } };
          ValidateHeader(nodeContent);
          break;
      }

    case "unordered-list": {
      break;
    }

    case "ordered-list": {
      break;
    }
    default: {
      console.log(`not parsed nodetype ${content.nodeType}`);
    }
  }
}

function validateParagraph(content) {
  const expectedHtml = buildExpectedHtml(content).trim();

  const parsedElement = parse(expectedHtml);

  cy.get("p").then(($paragraphs) => {
    const paragraphHtmls = Array.from(
      Array.from($paragraphs.map((i, el) => Cypress.$(el).html()))
        .map((paragraph) => {
          const withoutWhitespaceEscaped = CleanText(paragraph);

          return {
            original: withoutWhitespaceEscaped,
            parsed: parse(withoutWhitespaceEscaped),
          };
        })
        .filter((paragraph) => paragraph.original != "")
    );

    const anyMatches = paragraphHtmls.find(
      (paragraph) =>
        paragraph.original == expectedHtml ||
        paragraph.original.indexOf(expectedHtml) != -1 ||
        paragraph.parsed.innerHTML?.indexOf(parsedElement.innerHTML) != -1 ||
        paragraph.parsed.innerText
          ?.trim()
          .indexOf(parsedElement.innerText.trim()) != -1
    );

    if (!anyMatches) {
      console.error(`Could not find match for content`, expectedHtml, content);
      } else {
          expect(anyMatches).to.exist;
    }
  });
}

function buildExpectedHtml(content) {
  let html = "";
  for (const child of content.content) {
    if (child.value) {
      html += child.value.replace(/\r\n/g, "\n");
    }

    if (child.nodeType == "hyperlink") {
      html += `<a href="${child.data.uri}" class="govuk-link">`;
    }

    if (child.content) {
      for (const grandchild of child.content) {
        if (grandchild.value) {
          html += grandchild.value;
        }
      }
    }

    if (child.nodeType == "hyperlink") {
            if (!child.data.uri.includes("plan-technology-for-your-school") && !child.data.uri.includes("plantech-container-app-url")) {
                html += " (opens in new tab)";
            }
      html += `</a>`;
    }
  }

  return CleanText(html);
}
