import DataMapper from "../../../../contentful/export-processor/data-mapper";
import { contentful } from "./contentful";
import ValidatePage from "../helpers/content-validators/page-validator";
import { selfAssessmentSlug } from "../helpers/page-slugs";

describe("Pages should have content", () => {
  let dataMapper;

  before(() => {
    if (contentful && contentful.entries && contentful.entries.length > 0) {
      dataMapper = new DataMapper(contentful);
    }
  });

  it("Should work for unauthorised pages", () => {
    if (dataMapper?.pages == null) {
      console.log("Datamapper has not processed data correctly");
      return;
    }

    for (const [_, page] of dataMapper.pages) {
      if (page.fields.requiresAuthorisation || page.fields.slug != "/") {
        continue;
      }

      const slug = `/${page.fields.slug.replace("/", "")}`;
      cy.visit(slug);
      ValidatePage(slug, page);
    }
  });

  it.skip("Should validate self-assessment page", () => {
    if (dataMapper.pages == null) {
      console.log("Datamapper has not processed data correctly");
      return;
    }

    cy.loginWithEnv(`${selfAssessmentSlug}`);

    const selfAssessmentPage = FindPageForSlug({ slug, dataMapper });

    if (!selfAssessmentPage) {
      throw new Error(
        `Could not find self-assessment page; not found page with slug ${slug}`
      );
    }

    ValidatePage(slug, selfAssessmentPage);
  });

  it.skip("Should navigate through every question", () => {
    if (dataMapper.pages == null) {
      console.log("Datamapper has not processed data correctly");
      return;
    }

    cy.loginWithEnv(`${selfAssessmentSlug}`);

    const sections = dataMapper.mappedSections;

    for (const section of Object.values(sections)) {
      validateSections(section, section.minimumPathsToNavigateQuestions);
    }
  });
});

function validateSections(section, paths) {
  cy.visit(`/${selfAssessmentSlug}`);
  for (const path of paths) {
    //Navigate through interstitial page
    cy.get("ul.app-task-list__items > li a").contains(section.name).click();

    cy.url().should("include", section.interstitialPage.fields.slug);

    cy.get("a.govuk-button.govuk-link").contains("Continue").click();

    navigateAndValidateQuestionPages(path, section);

    validateCheckAnswersPage(path, section);

    cy.url().should("include", selfAssessmentSlug);
  }
}

function validateCheckAnswersPage(path, section) {
  for (const question of path) {
    cy.get("div.govuk-summary-list__row dt.govuk-summary-list__key.spacer")
      .contains(question.question.trim())
      .siblings("dd.govuk-summary-list__value.spacer")
      .contains(question.answer);

    cy.url().should(
      "include",
      `${section.interstitialPage.fields.slug}/check-answers`
    );
  }

  cy.get("button.govuk-button").contains("Save and continue").click();
}

function navigateAndValidateQuestionPages(path, section) {
  for (const question of path) {
    const matchingQuestion = section.questions.find(
      (q) => q.text === question.question
    );

    if (!matchingQuestion)
      throw new Error(
        `Couldn't find matching question for ${question.question}`
      );

    cy.url().should(
      "include",
      `/${section.interstitialPage.fields.slug}/${matchingQuestion.slug}`
    );

    //Contains question
    cy.get("h1.govuk-fieldset__heading").contains(question.question.trim());

    //Contains question help text
    if (matchingQuestion.helpText) {
      cy.get("div.govuk-hint").contains(matchingQuestion.helpText);
    }
    //Contains all answerees
    validateAnswers(matchingQuestion);

    //Select answer for path and continue
    cy.get(
      "div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label"
    )
      .contains(question.answer)
      .click();

    cy.get("button.govuk-button").contains("Save and continue").click();
  }
}

function validateAnswers(matchingQuestion) {
  for (const answer of matchingQuestion.answers) {
    cy.get(
      "div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label"
    ).contains(answer.text);
  }
}

function FindPageForSlug({ slug, dataMapper }) {
  for (const [id, page] of dataMapper.pages) {
    if (page.fields.slug == slug) {
      return page;
    }
  }

  return null;
}
