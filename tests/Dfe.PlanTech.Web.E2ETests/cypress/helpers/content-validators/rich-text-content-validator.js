import { parse } from "node-html-parser";

function ValidateRichTextContent(parent) {
  if (!parent?.content) return;

  for (const child of parent.content) {
    switch (child.nodeType) {
      case "paragraph": {
        validateParagraph(child);
      }
    }
  }
}

export default ValidateRichTextContent;
const regex = /(\r\n|\n|\r)/g;

function validateParagraph(content) {
  const children = Array.from(getChildrenRecursive(content));
  const values = children.map((child) => child.value);

  const htmlValue = getContentValues(values);

  if (!htmlValue || htmlValue.length == 0) return;

  cy.get("p").then(($p) => {
    const htmls = Array.from(
      $p.map((i, el) => {
        return Cypress.$(el).text();
      })
    );

    const matched = htmls.find((paragraph) =>
      htmlValue.every((html) => paragraph.indexOf(html) != -1)
    );

    if (matched) return;

    const parsed = htmlValue.map((html) => parse(html)).map((p) => p.innerText);

    const matchedHtmlContent = htmls
      .map((html) => html.replace(/\&nbsp;/g, ""))
      .some((paragraph) => {
        const matches = parsed.every((html) => {
          const m = paragraph.indexOf(html) != -1;
          return m;
        });

        return matches;
      });

    if (!matchedHtmlContent) {
      throw new Error(`Unable to find ${htmlValue}`);
    }
  });
}

function getContentValues(values) {
  return Array.from(
    values
      .filter((value) => value != null && value != "<br>")
      .map((value) => value.trim())
      .filter((value) => value != null)
      .flatMap((value) => {
        if (value.match(regex)) {
          return value.split(regex);
        } else {
          return [value];
        }
      })
      .filter(
        (value) => value && value != "" && value != "\n" && value != "\r\n"
      )
  );
}

function* getChildrenRecursive(content) {
  if (content.content) {
    for (const child of content.content) {
      yield child;

      yield* getChildrenRecursive(child);
    }
  }
}
