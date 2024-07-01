import ValidatePage from "../helpers/content-validators/page-validator.js";
import { selfAssessmentSlug } from "../helpers/page-slugs.js";
import ValidateContent from "../helpers/content-validators/content-validator.js";
import DataMapper from "export-processor/data-mapper.js";

describe("Pages should have content", () => {

  const dataMapper = new DataMapper(require('../fixtures/contentful-data'));

  before(async () => {
  });

  it("Should render navigation links", async () => {
    if (!dataMapper) {
      return;
    }

    const navigationLinks = Array.from(dataMapper.contents.get("navigationLink").entries());
    if (!dataLoaded(navigationLinks)) {
      return;
    }

    cy.visit("/");

    for (const [_, navigationLink] of navigationLinks) {
      ValidateContent(navigationLink);
    }
  });

  it("Should validate self-assessment page", () => {
    if (!dataMapper || !dataLoaded(dataMapper.pages)) {
      return;
    }

    const slug = selfAssessmentSlug.replace("/", "");
    const selfAssessmentPage = FindPageForSlug({
      slug,
      dataMapper
    });

    if (!selfAssessmentPage) {
      throw new Error(
        `Could not find self-assessment page; not found page with slug ${selfAssessmentSlug}`
      );
    }

    cy.loginWithEnv(selfAssessmentSlug);

    ValidatePage(slug, selfAssessmentPage);
  });

  Array.from(dataMapper?.pages ?? [])
    .map(([_, page]) => page)
    .filter((page) => !page.fields.requiresAuthorisation)
    .forEach((page) => {
      it(
        "Should have correct content on non-authorised pages. Testing " +
        page.fields.internalName,
        () => {
          const slug = `/${page.fields.slug.replace("/", "")}`;
          cy.visit(slug);
          ValidatePage(slug, page);
        }
      );
    });


  (dataMapper?.mappedSections ?? []).forEach((section) => {
    it(`${section.name} should have every question with correct content`, () => {
      cy.loginWithEnv(`${selfAssessmentSlug}`);

      validateSections(
        section,
        section.minimumPathsToNavigateQuestions,
        dataMapper
      );
    });

    Object.keys(section.minimumPathsForRecommendations).forEach(
      (maturity) => {
        it(`${section.name} should retrieve correct recommendation for ${maturity} maturity, and all content is valid`, () => {
          cy.loginWithEnv(`${selfAssessmentSlug}`);

          const matchingSection = dataMapper.mappedSections.find(s => s.id == section.id);
                    
          validateSections(matchingSection, section.minimumPathsForRecommendations[maturity], dataMapper, () => {
            validateRecommendationForMaturity(matchingSection, maturity);
          });
        });
      }
    );
  });
});


/**
 * Check if data has been loaded and log a message if not.
 *
 * @param {Array | Map} contentMap - the map of content
 * @return {boolean} haveContent - whether data has been loaded
 */
function dataLoaded(contentMap) {
  const haveContent = contentMap && ((contentMap.size && contentMap.size > 0) || (contentMap.length && contentMap.length > 0));

  if (!haveContent) {
    console.log("Data has not been loaded");
  }

  return haveContent;
}

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
 
  const matchingRecommendation = section.recommendation.intros.find(
    (recommendation) => recommendation.maturity == maturity
  );

  if (!matchingRecommendation)
    throw new Error(
      `Couldn't find a recommendation for maturity ${maturity} in ${section.name}`,
      section
    );

  const expectedPath =
    `/${section.name.trim()}/recommendation/${matchingRecommendation.slug.trim()}`
      .toLowerCase()
      .replace(/ /g, "-");

  cy.get(
    "ul.app-task-list__items li.app-task-list__item span.app-task-list__task-name a.govuk-link"
  )
    .contains(section.name.trim())
    .should("have.attr", "href")
    .and("include", expectedPath);

  cy.get(
    "ul.app-task-list__items li.app-task-list__item span.app-task-list__task-name a.govuk-link"
  )
    .contains(section.name.trim())
    .click();

  ValidatePage(
    matchingRecommendation.slug,
    matchingRecommendation.page
  );
}

/**
 * Validates sections using the given paths
 *
 * @param {Object} section - the section to validate
 * @param {Array} paths - the paths to navigate
 * @param {Function} validator - optional validation function to call at the end of every path
 */
function validateSections(section, paths, dataMapper, validator) {
  cy.visit(`/${selfAssessmentSlug}`);

    for (const path of paths) {
    //Navigate through interstitial page
      cy.get("div.govuk-summary-list__row > dt a").contains(section.name).click();

    cy.url().should("include", section.interstitialPage.fields.slug);

    const interstitialPage = FindPageForSlug({
      slug: section.interstitialPage.fields.slug,
      dataMapper,
    });

    ValidatePage(section.interstitialPage.fields.slug, interstitialPage);

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

    for (let i = 0; i < path.length; i++) {
        cy.task("log", path[i].question.text)
        cy.get("div.govuk-summary-list__row dt.govuk-summary-list__key.spacer")
      .contains(path[i].question.text.trim())
      .siblings("dd.govuk-summary-list__value.spacer")
      .contains(path[i].answer.text);
    cy.url().should(
      "include",
      `${section.interstitialPage.fields.slug}/check-answers`
    );
    }

  cy.get("button.govuk-button").contains("Save and continue").click();
}

function navigateAndValidateQuestionPages(path, section) {


    for (let i = 0; i < path.length; i++) {
    const matchingQuestion = section.questions.find(
        (q) => q.text === path[i].question.text
    );
    
    if (!matchingQuestion) {
      throw new Error(
        `Couldn't find matching question for ${path[i].question.text}`
      );
    }
    
    cy.url().should(
      "include",
      `/${section.interstitialPage.fields.slug}/${matchingQuestion.slug}`
    );

    //Contains question
    cy.get("h1.govuk-fieldset__heading").contains(path[i].question.text.trim());

    //Contains question help text
    if (matchingQuestion.helpText) {
      cy.get("div.govuk-hint").contains(matchingQuestion.helpText);
    }
    //Contains all answers
    validateAnswers(matchingQuestion);

    //Select answer for path and continue
    cy.get(
      "div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label"
    )
      .contains(path[i].answer.text)
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
  for (const [id, page] of dataMapper.pages.entries()) {
    if (page.fields.slug == slug) {
      return page;
    }
  }

  return null;
}
