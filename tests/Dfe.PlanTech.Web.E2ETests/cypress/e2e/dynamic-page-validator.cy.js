import ValidatePage from "../helpers/content-validators/page-validator.js";
import { selfAssessmentSlug } from "../helpers/page-slugs.js";
import ValidateContent from "../helpers/content-validators/content-validator.js";
import DataMapper from "export-processor/data-mapper.js";

describe("Pages should have content", () => {
  let contentfulData;

  before(async () => {
    const contentfulExport = await cy.task("fetchContentfulData");
    console.log('loaded contentful data', contentfulExport);
    contentfulData = new DataMapper(contentfulExport);
  });

  it("Should render navigation links", async () => {
    if (!contentfulData) {
      return;
    }
    console.log('contentful data', contentfulData);

    const navigationLinks = contentfulData.contents.get("navigationLink");
    console.log('nav links', navigationLinks);
    if (!dataLoaded(navigationLinks)) {
      return;
    }

    cy.visit("/");

    for (const [_, navigationLink] of navigationLinks) {
      ValidateContent(navigationLink);
    }
  });

  it("Should validate self-assessment page", () => {
    if (!contentfulData || !dataLoaded(contentfulData.pages)) {
      return;
    }

    cy.loginWithEnv(selfAssessmentSlug);
    const slug = selfAssessmentSlug.replace("/", "");
    const selfAssessmentPage = FindPageForSlug({
      slug,
      dataMapper: contentfulData,
    });

    if (!selfAssessmentPage) {
      throw new Error(
        `Could not find self-assessment page; not found page with slug ${selfAssessmentSlug}`
      );
    }

    ValidatePage(slug, selfAssessmentPage);
  });

  Array.from(contentfulData?.pages ?? [])
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

  Object.values(contentfulData?.mappedSections ?? []).forEach((section) => {
    it(`${section.name} should have every question with correct content`, () => {
      cy.loginWithEnv(`${selfAssessmentSlug}`);

      validateSections(
        section,
        section.minimumPathsToNavigateQuestions,
        contentfulData
      );
    });

    Object.entries(section.minimumPathsForRecommendations).forEach(
      ([maturity, path]) => {
        it(`${section.name} should retrieve correct recommendation for ${maturity} maturity, and all content is valid`, () => {
          cy.loginWithEnv(`${selfAssessmentSlug}`);

          validateSections(section, [path], contentfulData, () => {
            validateRecommendationForMaturity(section, maturity);
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
  const haveContent = contentMap && contentMap.size > 0;

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
    .should("have.attr", "href")
    .and("include", expectedPath);

  cy.get(
    "ul.app-task-list__items li.app-task-list__item span.app-task-list__task-name a.govuk-link"
  )
    .contains(matchingRecommendation.displayName.trim())
    .click();

  ValidatePage(
    matchingRecommendation.page.fields.slug,
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
    cy.get("ul.app-task-list__items > li a").contains(section.name).click();

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
