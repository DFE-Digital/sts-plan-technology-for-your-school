import { Question } from "./question.js";
import { UserJourney } from "../user-journey/user-journey.js";
import SubtopicRecommendation from "./subtopic-recommendation.js";
import ErrorLogger from "../errors/error-logger.js";

export class Section {
  recommendation;
  questions;
  name;
  interstitialPage;

  id;

  paths;
  minimumPathsToNavigateQuestions;
  minimumPathsForRecommendations;

  /**
   * 
   * @param {Object} params
   * @param {Object} params.fields - The fields of the section
   * @param {SubtopicRecommendation} recommendation - Subtopic recommendation for the seciton
   */
  constructor({ fields, sys }, recommendation) {
    this.interstitialPage = fields.interstitialPage;
    this.questions = fields.questions.map((question) => new Question(question));
    this.id = sys.id;
    this.name = fields.name;
    this.recommendation = recommendation;

    this.paths = this.getAllPaths(this.questions[0]).map((path) => {
      const userJourney = new UserJourney(path, this);
      userJourney.setRecommendation(recommendation);

      return userJourney;
    });

    this.getMinimumPathsForQuestions();
    this.getMinimumPathsForRecommendations();

    this.setNextQuestions();
  }

  /**
   * Find the minimum amount of paths possible that allows a user to navigate through every question.
   */
  getMinimumPathsForQuestions() {
    const sortedPaths = this.paths
      .slice()
      .sort((a, b) => b.path.length - a.path.length);

    this.minimumPathsToNavigateQuestions = this.calculateMinimumPaths(
      sortedPaths,
      this.questions
    );
  }

  /**
   * Calculate the minimum amount of paths possible for each recommendation.
   * Also tries to ensure the paths are as sort as possible.
   */
  getMinimumPathsForRecommendations() {
    const pathsShortedByShortestFirst = this.paths
      .slice()
      .sort((a, b) => a.path.length - b.path.length);
    this.minimumPathsForRecommendations =
      this.calculateMinimumPathsForRecommendations(pathsShortedByShortestFirst);
  }

  /**
   * Calculate the minimum paths to answer all target questions.
   *
   * @param {array} sortedPaths - The sorted paths to be considered.
   * @param {array} targetQuestions - The target questions to be answered.
   * @return {array} An array containing the minimum paths to answer the target questions.
   */
  calculateMinimumPaths(sortedPaths, targetQuestions) {
    const remainingQuestions = targetQuestions.map((question) => question.id);
    let minimumPaths = [];

    const uniquePaths = this.getUniquePaths(sortedPaths);

    // Find minimum paths to answer all questions
    for (const path of uniquePaths) {
      const matchingQuestions = remainingQuestions.filter((questionId) =>
        path.some((id) => id == questionId)
      );

      if (matchingQuestions.length > 0) {
        minimumPaths.push(path);

        for (const questionId of path) {
          removeFromArray(remainingQuestions, questionId);
        }
      }
    }

    minimumPaths = minimumPaths.map((minPath) => {
      const matchingPath = this.paths.find((path) =>
        minPath.every((questionId) =>
          path.path.some((pathPart) => pathPart.question.id == questionId)
        )
      );

      if (!matchingPath) {
        console.error(`Couldn't find matching path`, minPath);
        return;
      }

      return matchingPath.path;
    });

    // If there are remaining questions, find paths for each question
    for (const questionId of remainingQuestions) {
      const pathContainingQuestion = this.getFirstPathContainingQuestion(
        sortedPaths,
        questionId
      );

      if (pathContainingQuestion == null) continue;

      minimumPaths.push(pathContainingQuestion.path);
    }

    return minimumPaths;
  }

  /**
   * Calculate the minimum paths for recommendations.
   *
   * @param {Array} paths - The paths to calculate minimums for
   * @return {Object} minimumPathsForRecommendations - The calculated minimum paths for recommendations
   */
  calculateMinimumPathsForRecommendations(paths) {
    const possibleMaturities = ["Low", "Medium", "High"];
    const minimumPathsForRecommendations = {};

    for (const maturity of possibleMaturities) {
      const pathForRecommendation = paths.find(
        (path) => path.maturity == maturity
      );

      if (!pathForRecommendation) {
        ErrorLogger.addError({ id: this.id, contentType: "section", message: `No path exists for ${maturity}` });
        continue;
      }

      minimumPathsForRecommendations[maturity] =
        pathForRecommendation.path;
    }

    return minimumPathsForRecommendations;
  }

  /**
   * Calculates the statistics of the paths.
   * Currently just counts the number of paths for each maturity level.
   */
  get stats() {
    return this.paths.reduce((count, path) => {
      const maturity = path.maturity;
      return count[maturity] ? ++count[maturity] : (count[maturity] = 1), count;
    }, {});
  }

  /**
   * Retrieves all possible paths from the current question.
   *
   * @param {Object} currentQuestion - The current question object
   * @return {Array} An array of all possible paths
   */
  getAllPaths(currentQuestion) {
    const paths = [];
    const stack = [];

    stack.push({
      currentPath: [],
      currentQuestion,
    });

    while (stack.length > 0) {
      const { currentPath, currentQuestion } = stack.pop();

      if (!currentQuestion) {
        paths.push(currentPath);
        continue;
      }

      currentQuestion.answers.forEach((answer) => {
        const newPath = [
          ...currentPath,
          { question: currentQuestion, answer: answer },
        ];

        const nextQuestion = this.questions.find(
          (q) => q.id === answer.nextQuestion?.sys.id
        );

        stack.push({
          currentPath: newPath,
          currentQuestion: nextQuestion,
        });
      });
    }

    return paths;
  }

  /**
   * Get unique paths from the sorted paths array.
   *
   * @param {Array} sortedPaths - The array of sorted paths
   * @return {Array} The array of unique paths
   */
  getUniquePaths(sortedPaths) {
    const pathsWithQuestions = sortedPaths.map((path) => ({
      path: path.path,
      questionsAnswered: path.path.map((pathPart) => pathPart.question.id),
    }));

    const uniqueQuestions = new Set(
      pathsWithQuestions.map((path) => JSON.stringify(path.questionsAnswered))
    );

    const uniquePaths = Array.from(uniqueQuestions).map(JSON.parse);
    return uniquePaths;
  }

  /**
   * Find the first path containing the specified question ID.
   *
   * @param {array} sortedPaths - The array of sorted paths to search through.
   * @param {number} questionId - The ID of the question to find in the paths.
   * @return {object} The path containing the specified question ID, or undefined if not found.
   */
  getFirstPathContainingQuestion(sortedPaths, questionId) {
    // Find the first path that contains the remaining question
    const pathsForQuestion = sortedPaths.find((path) =>
      path.path.some((pathPart) => pathPart.question.id == questionId)
    );

    if (pathsForQuestion) {
      return pathsForQuestion;
    }

    const question = this.questions.find(
      (question) => question.id == questionId
    );

    if (!question) {
      ErrorLogger.addError({ id: this.id, contentType: "section", message: `Couldn't find question ${questionId} in ${this.name}` });
      return;
    }

    ErrorLogger.addError({
      id: this.id, contentType: "section", message: `Question ${questionId} does not have a path`
    });

    return;
  }

  /**
   * For each answer, in each question (in the this.questions property),
   * find the matching question for the 'nextQuestion' property, and set it if found
   */
  setNextQuestions() {
    for (const question of this.questions) {
      for (const answer of question.answers) {
        const nextQuestionId = answer.nextQuestion?.sys.id;
        if (nextQuestionId == null) {
          continue;
        }

        const matchingQuestions = this.questions.filter(
          (q) => q.id == nextQuestionId
        );

        if (matchingQuestions.length == 0) {
          console.error(
            `Error finding question for ${nextQuestionId} in ${this.name} - answer ${answer.text} ${answer.id} in ${question.text} ${question.id}`
          );

          answer.nextQuestion = null;
          continue;
        }

        answer.nextQuestion = matchingQuestions[0];
      }
    }
  }
}

/**
 * Removes the first occurrence of the specified value from the array.
 *
 * @param {Array} arr - The array to remove the value from
 * @param {any} value - The value to be removed from the array
 * @return {Array} The array with the first occurrence of the value removed
 */
const removeFromArray = (arr, value) => {
  var index = arr.indexOf(value);
  if (index > -1) {
    arr.splice(index, 1);
  }
  return arr;
};
