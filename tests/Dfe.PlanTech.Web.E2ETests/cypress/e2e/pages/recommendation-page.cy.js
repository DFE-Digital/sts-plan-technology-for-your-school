let selectedQuestionsWithAnswers = [];

describe("Recommendation Page", () => {
    const url = "/self-assessment";

    beforeEach(() => {
        selectedQuestionsWithAnswers = [];

        cy.loginWithEnv(url);

        navigateToRecommendationPage();

        cy.url().should("contain", "recommendation");

        cy.injectAxe();
    });

    it("Should Have Heading", () => {
        cy.get("h1.govuk-heading-xl")
            .should("exist")
    });

    it("Should Have Back Button", () => {
        cy.get('a:contains("Back")')
            .should("exist")
            .should("have.attr", "href")
            .and("include", "/self-assessment")
    });

    it("Should Have Content", () => {
        cy.get("rich-text").should("exist");
    });

    it("Passes Accessibility Testing", () => {
        cy.runAxe();
    });

});

const navigateToRecommendationPage = () => {
    cy.clickFirstSection();
    cy.clickContinueButton();

    return navigateThroughCheckAnswersPage().then((res) => cy.wrap(res)).then(() => submitAnswers()).then((onSelfAssessmentPage) => {
        if (!onSelfAssessmentPage) {
            return Promise.resolve();
        }

        cy.get('a[href*="/recommendation/"]').first().click();
    });
};

const navigateThroughCheckAnswersPage = () => {
    return navigateThroughQuestions().then(() => cy.wrap(selectedQuestionsWithAnswers));
};

const navigateThroughQuestions = () => {
    return cy
        .get("main")
        .then(($main) => $main.find("form div.govuk-radios").length > 0)
        .then((onQuestionPage) => {
            if (!onQuestionPage) {
                return Promise.resolve();
            }

            cy.selectFirstRadioButton().then((questionWithAnswer) =>
                selectedQuestionsWithAnswers.push(questionWithAnswer)
            );
            cy.saveAndContinue();

            return navigateThroughQuestions();
        })
        .then(() => cy.wrap(selectedQuestionsWithAnswers));
};

const submitAnswers = () =>
    cy.get("button.govuk-button").contains("Save and submit").click();