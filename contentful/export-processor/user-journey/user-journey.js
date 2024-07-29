import SubtopicRecommendation from "../content-types/subtopic-recommendation.js";
import ErrorLogger from "../errors/error-logger.js";

export class UserJourney {
  path;
  maturity;
  section;
  recommendationIntro;

  get pathWithTextOnly() {
    return this.mapPathToOnlyQuestionAnswerTexts();
  }

  constructor(path, section) {
    this.path = path;
    this.section = section;

    this.maturity = this.maturityRanking(
      path
        .map((questionAnswer) => questionAnswer.answer.maturity)
        .filter(onlyUnique)
        .map(this.maturityRanking)
        .filter((maturity) => maturity != null)
        .sort()[0]
    );

    if (!this.maturity) {
      ErrorLogger.addError({ id: "", contentType: "User journey", message: this.pathToErrorMessage() });
    }
  }

  pathToErrorMessage() {
    return this.path.map((pathPart) => `Question "${pathPart.question.text}" - "${pathPart.answer.text}"`).join(" -> ");
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

    this.recommendationIntro = recommendationIntro;
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
   * @param {type} pathPart - description of parameter
   * @return {type} the mapped question and answer texts
   */
  mapPathToOnlyQuestionAnswerTexts() {
    return this.path.map((pathPart) => {
      return {
        question: pathPart.question.text,
        answer: pathPart.answer.text,
      };
    });
  }
}

const onlyUnique = (value, index, array) => array.indexOf(value) === index;
