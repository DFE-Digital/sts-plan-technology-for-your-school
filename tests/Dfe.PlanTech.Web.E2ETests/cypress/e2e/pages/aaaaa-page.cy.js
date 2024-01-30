import DataMapper from "./contentful-helpers/data-mapper.mjs";

import { contentful } from "./contentful";

describe("Pages should have content", () => {
  let dataMapper;

  before(() => {
    dataMapper = new DataMapper(contentful);
  });

  it("Should work for unauthorised pages", () => {
    for (const [pageId, page] of dataMapper.pages) {
      if (page.fields.requiresAuthorisation) {
        continue;
      }

      const slug = `/${page.fields.slug.replace("/", "")}`;
      cy.visit(slug);
      ShouldMatchUrl(slug);

      if (page.fields.title) {
        validateTitle(page);
      }

      const contents = page.fields.content;

      for (const content of contents) {
        switch (content.sys.contentType.sys.id) {
          case "header": {
            testHeader(content);
            break;
          }
          case "buttonWithLink": {
            testButtonWithLink(content);
            break;
          }
          case "textBody": {
            for (const textContent of content.fields.richText.content) {
              validateTextContent(textContent);
            }
          }
          default: {
            console.log(content.sys.contentType.sys.id, content);
            break;
          }
        }
      }
    }
  });
});
function validateTextContent(textContent) {
  if (textContent.nodeType == "paragraph") {
    const innerTexts = textContent.content.map((c) => c.value);
    cy.get("p").should(($p) => {
      for (var x = 0; x < $p.length; x++) {
        const innerText = $p[x].innerText;
        const matches = innerTexts.every(
          (text) => innerText.indexOf(text) > -1
        );

        console.log(x, innerText, matches, innerTexts);
        if (matches) {
          for (const t of innerTexts) {
            expect($p.eq(x)).to.contain(t);
          }

          console.log("matched", innerText, innerTexts);
          expect(matches).to.be(true);
        }
      }

      throw new Error(`Couldn't find matching text for content`, textContent);
    });
  }
  if (textContent.value && textContent.nodeType == "nodeType") {
    //console.log("text");
  } else {
    // console.log(
    //   "not text",
    //   textContent.value,
    //   textContent.nodeType,
    //   textContent
    // );
  }

  if (textContent.content) {
    for (const content of textContent.content) {
      validateTextContent(content);
    }
  }
}

function testButtonWithLink(content) {
  const classAssertion = content.fields.button.fields.isStartButton
    ? "have.class"
    : "not.have.class";

  cy.get("a.govuk-button")
    .contains(content.fields.button.fields.value)
    .and(classAssertion, "govuk-button--start");
}

function validateTitle(page) {
  cy.get("h1.govuk-heading-xl")
    .should("exist")
    .and("have.text", page.fields.title.fields.text);
}

function testHeader(content) {
  const tag = content.fields.tag;
  const size = content.fields.size.toLowerCase();
  const sizeLetters =
    content.fields.size == "extra large" ? "xl" : size.substring(0, 1);

  const expectedClass = "govuk-heading-" + sizeLetters;

  cy.get(tag).contains(content.fields.text).should("have.class", expectedClass);
}

function ShouldMatchUrl(url) {
  cy.location().should((loc) => {
    expect(loc.pathname).to.equal(url);
  });
}

function TestContent() {}

function ShouldBeAuthorised(page) {
  cy.request({
    url: page.fields.slug,
    followRedirect: false, // turn off following redirects
  }).then((resp) => {
    // redirect status code is 302
    expect(resp.status).to.eq(302);
    expect(resp.redirectedToUrl).to.contain(
      "https://pp-oidc.signin.education.gov.uk/"
    );
  });
}

function removeHtmlTags(str) {
  return str.replace(/(<([^>]+)>)/gi, "");
}
