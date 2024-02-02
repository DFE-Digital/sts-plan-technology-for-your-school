class RichTextContentValidator {
  validateContent() {
    //For each content ->
    //Build HTML for that content
    //Then loop through children and add HTML
    //When complete
    //Find matching content
  }
}

function ValidateRichTextContent(content) {
  console.log(content);
  const parent = content.content;

  let html = "";
  let lastNodeType = parent.nodeType;
  for (const child of content.content) {
    newFunction(child);
  }
}

export default ValidateRichTextContent;

function newFunction(child) {
  const grandChildren = Array.from(GetAllContentsRecursive(child));

  switch (child.nodeType) {
    case "paragraph": {
      const expectedText = grandChildren
        .map((c) => c.value)
        .join("")
        .trim();

      if (expectedText == null || expectedText == "") return;

      cy.get("p").contains(expectedText);

      break;
    }

    default: {
      console.log("not mapped case " + child.nodeType);
      break;
    }
  }
}

function* GetAllContentsRecursive(richTextContent) {
  if (richTextContent.content) {
    for (const child of richTextContent.content) {
      yield child;
    }
  }
}

//TODO: Move to class
//TODO: Generate expected HTMl based on tag helpers and check for that. OR start from the bottom -> up.
function validateTextContent(textContent) {
  if (textContent.nodeType == "paragraph") {
    const innerTexts = Array.from(GetAllContentsRecursive(textContent))
      .filter((content) => content.value != null)
      .map((content) => content.value)
      .join("")
      .trim();

    if (innerTexts == null || innerTexts == "") return;

    cy.get("p").then(($p) => {
      var matches = false;
      for (const paragraph of $p) {
        matches =
          paragraph.innerHTML.indexOf(innerTexts) != -1 ||
          paragraph.innerText.indexOf(innerTexts) != -1;

        if (matches) {
          break;
        }
      }

      if (!matches) {
        console.error(`Couldn't find text`, innerTexts);
        throw "not found";
      }
    });
  }
  if (textContent.nodeType == "table-header-cell") {
    const contents = GetAllContentsRecursive(textContent);

    const text = contents
      .filter((content) => content.value)
      .map((content) => content.value)
      .join(" ");

    cy.get("th.govuk-table__header").should("have.text", text);
  } else if (
    textContent.value &&
    textContent.nodeType == "table-header-unordered-list"
  ) {
  } else if (textContent.value && textContent.nodeType == "list-item") {
  } else {
    // console.log(
    //   "not text",
    //   textContent.value,
    //   textContent.nodeType,
    //   textContent
    // );
  }

  /*TYPES:
    'paragraph',
  'text',
  'unordered-list',
  'list-item',
  'hyperlink',
  'ordered-list',
  'table',
  'table-row',
  'table-header-cell',
  'table-cell'
  */
}
