import ErrorLogger from "../errors/error-logger.js";
// eslint-disable-next-line no-unused-vars
import SubtopicRecommendation from "../content-types/subtopic-recommendation.js";

export class UserJourney {
    _maturity;
    _questionIdsAnswered;

    path;
    section;
    recommendationIntro;

    get pathWithTextOnly() {
        return this.mapPathToOnlyQuestionAnswerTexts();
    }

    constructor(path, section) {
        this.path = path;
        this.section = section;
    }

    /**
     *
     */
    get maturity() {
        if (this._maturity !== undefined) {
            return this._maturity;
        }

        this._maturity = this.calculateMaturity();

        if (!this._maturity) {
            ErrorLogger.addError({
                id: "",
                contentType: "User journey",
                message: this.pathToString,
            });
        }

        return this._maturity;
    }

    /**
     * @returns {string[]}
     */
    get questionIdsAnswered() {
        if (this._questionIdsAnswered !== undefined) {
            return this._questionIdsAnswered;
        }

        this._questionIdsAnswered = this.path.map(
            (pathPart) => pathPart.question.id
        );

        return this._questionIdsAnswered;
    }

    get pathToString() {
        return this.path
            .map(
                (pathPart) =>
                    `Question "${pathPart.question.text}" - "${pathPart.answer.text}"`
            )
            .join(" -> ");
    }

    /**
     * Finds and sets the recommendation property from the recommendations received
     *
     * @param {SubtopicRecommendation} recommendation - the list of recommendations to look through
     */
    setRecommendation(recommendation) {
        if (!this.maturity) {
            return;
        }

        const recommendationIntro = recommendation.intros.filter(
            (intro) => intro.maturity == this.maturity
        );

        if (recommendationIntro == null || recommendationIntro.length == 0) {
            ErrorLogger.addError({
                id: recommendation.id,
                contentType: "recommendation",
                message: `Could not find recommendation intro for ${this.maturity} in ${this.section.name}`,
            });
            return;
        }

        this.recommendationIntro = recommendationIntro[0];
    }

    /**
     * A function that maps maturity levels to integers, for comparing/sorting easily.
     *
     * @param {string|number} maturity - the maturity level to be ranked
     * @return {number|string|null} the ranked maturity level or null if not found
     */
    maturityRanking(maturity) {
        switch (maturity) {
            case "Low":
                return 0;
            case "Medium":
                return 1;
            case "High":
                return 2;
            case 0:
                return "Low";
            case 1:
                return "Medium";
            case 2:
                return "High";
        }

        return null;
    }

    /**
     * Maps the path to only the question and answer texts.
     *
     * @return {questionAnswerText[]} the mapped question and answer texts
     */
    mapPathToOnlyQuestionAnswerTexts() {
        return this.path.map((pathPart) => {
            return {
                question: pathPart.question.text,
                answer: pathPart.answer.text,
            };
        });
    }

    calculateMaturity() {
        return this.path
            .map((questionAnswer) => questionAnswer.answer.maturity)
            .filter(onlyUnique)
            .filter((maturity) => maturity != null)
            .sort()[0];
    }

    /**
     *
     * @returns {UserJourneyMinimalOutput}
     */
    toMinimalOutput() {
        return {
            recommendation:
                this.recommendationIntro != null
                    ? {
                          name: this.recommendationIntro?.displayName,
                          maturity: this.maturity,
                      }
                    : null,
            path: this.pathWithTextOnly,
        };
    }
}

const onlyUnique = (value, index, array) => array.indexOf(value) === index;

/**
 * @typedef {Object} questionAnswerText
 * @property {string} question
 * @property {string} answer
 */

/**
 * @typedef {Object} UserJourneyMinimalOutput
 * @property {({ name: string, maturity: string} | undefined)} recommendation
 * @property {questionAnswerText[]} path
 */
