import DataMapper from "../../../../contentful/export-processor/data-mapper";
import { contentful } from "./contentful-testing";
import ValidatePage from "../helpers/content-validators/page-validator";
import { selfAssessmentSlug } from "../helpers/page-slugs";

describe("Pages should have content", () => {
  let dataMapper;

  before(() => {
    if (contentful && contentful.entries && contentful.entries.length > 0) {
      dataMapper = new DataMapper(contentful);
    }
  });

  it.skip("Should work for unauthorised pages", () => {
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

  it("Should retrieve correct recommendations for maturity", () => {
    if (dataMapper.pages == null) {
      console.log("Datamapper has not processed data correctly");
      return;
    }

    cy.loginWithEnv(`${selfAssessmentSlug}`);

    const sections = dataMapper.mappedSections;

    for (const section of Object.values(sections)) {
      for (const [maturity, path] of Object.entries(
        section.minimumPathsForRecommendations
      )) {
        validateSections(section, [path], () => {
          validateRecommendationForMaturity(section, maturity);
        });
      }
    }
  });
});

/**
 * Validates a recommendation for a specific maturity level.
 *
 * @param {Object} section - the section containing the recommendations
 * @param {string} maturity - the maturity level to validate
 * @throws {Error} if matching recommendation is not found
 */
function validateRecommendationForMaturity(section, maturity) {
  //Validate recommendation banner
  cy.get(
    "div.govuk-notification-banner.govuk-notification-banner--success h3.govuk-notification-banner__heading"
  ).contains(`You have one new recommendation for ${section.name}`);

  const matchingRecommendation = section.recommendations.find(
    (recommendation) => recommendation.maturity == maturity
  );

  if (!matchingRecommendation)
    throw new Error(
      `Couldn't find a recommendation for maturity ${maturity} in ${section.name}`,
      section
    );

  const expectedPath =
    `/${section.name.trim()}/recommendation/${matchingRecommendation.page.fields.slug.trim()}`
      .toLowerCase()
      .replace(/ /g, "-");

  cy.get(
    "ul.app-task-list__items li.app-task-list__item span.app-task-list__task-name a.govuk-link"
  )
    .contains(matchingRecommendation.displayName.trim())
    .and("have.attr", "href")
    .and("include", expectedPath);
}

/**
 * Validates sections using the given paths
 *
 * @param {Object} section - the section to validate
 * @param {Array} paths - the paths to navigate
 * @param {Function} validator - optional validation function to call at the end of every path
 */
function validateSections(section, paths, validator) {
  cy.visit(`/${selfAssessmentSlug}`);

  for (const path of paths) {
    //Navigate through interstitial page
    cy.get("ul.app-task-list__items > li a").contains(section.name).click();

    cy.url().should("include", section.interstitialPage.fields.slug);

    cy.get("a.govuk-button.govuk-link").contains("Continue").click();

    navigateAndValidateQuestionPages(path, section);

    validateCheckAnswersPage(path, section);

    cy.url().should("include", selfAssessmentSlug);

    if (validator) {
      validator();
    }
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
