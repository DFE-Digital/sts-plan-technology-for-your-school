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
        userJourney.setRecommendation(recommendation, path);
      return userJourney;
    });

      this.getMinimumPathsForQuestions();
      this.getMinimumPathsForRecommendations();
      this.getPathsForAllAnswers();
      this.setNextQuestions();
      this.checkAllChunksTested();
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
   * Calcutes paths to select each answer to each question
   */

    getPathsForAllAnswers() {
        const allAnswerPaths = []

        // Log ids of all answers in section
        const allAnswers = this.questions.flatMap(question => question.answers.map(answer => answer.id));

        // Log ids of answers used in minimum paths for recommendations
        const allRecommendationPaths = this.minimumPathsForRecommendations;
        const answersUsed = Object.values(allRecommendationPaths).flat().reduce((previous, current) => {
            if (previous.indexOf(current.answer.id) == -1) {
                previous.push(current.answer.id);
            }
            return previous;
        }, []);

        // Create paths until all answers are used
        while (answersUsed.length !== allAnswers.length) {
            const neededNextQuestions = [];
            const newPath = [];
            const lastQuestion = this.questions[this.questions.length - 1];
            let currentAnswer;

            // Add last question and answer
            const unusedLastAnswers = lastQuestion.answers.filter(answer => !answersUsed.includes(answer.id))

            if (unusedLastAnswers.length) {
                newPath.unshift({ question: lastQuestion, answer: unusedLastAnswers[0] })
                answersUsed.push(unusedLastAnswers[0].id)
                neededNextQuestions.unshift(lastQuestion.id);
            } else {
                newPath.unshift({ question: lastQuestion, answer: lastQuestion.answers[0] })
            }

            // Loop through remaining questions
            for (let i = this.questions.length - 2; i >= 0; i--) {
                const currentQuestion = this.questions[i]
                if (neededNextQuestions.length) {
                    const validAnswers = currentQuestion.answers.filter(answer => answer.nextQuestion && answer.nextQuestion.sys.id == neededNextQuestions[0])
                    const validUnusedAnswers = validAnswers.filter(answer => !answersUsed.includes(answer.id))

                    currentAnswer = validUnusedAnswers.length ? validUnusedAnswers[0] : validAnswers[0]

                    if (validUnusedAnswers.length) {
                        answersUsed.push(currentAnswer.id)
                    }

                    neededNextQuestions.unshift(currentQuestion.id)

                } else {
                    const unusedAnswers = currentQuestion.answers.filter(answer => !answersUsed.includes(answer.id))

                    currentAnswer = unusedAnswers.length ? unusedAnswers[0] : currentQuestion.answers[0]

                    if (unusedAnswers.length) {
                        neededNextQuestions.unshift(currentQuestion.id)
                        answersUsed.push(currentAnswer.id)
                    }
                }

                newPath.unshift({ question: currentQuestion, answer: currentAnswer })
            }

            // Check through new path and remove later questions if nextQuestion is undefined (ie path contains early answer that shortens the user journey)
            for (let i = 0; i < newPath.length; i++) {
                if (newPath[i].answer && !newPath[i].answer.nextQuestion) {
                    newPath.splice(i + 1, newPath.length - i + 1)
                    break;
                } else if (newPath[i].answer && newPath[i].answer.nextQuestion.sys.id !== newPath[i + 1].question.id) {
                    const nextQuestion = newPath.findIndex((question) => question.question.id === newPath[i].answer.nextQuestion.sys.id, i + 1)
                    newPath.splice(i + 1, nextQuestion - (i + 1))
                    continue;
                }
            }
            allAnswerPaths.push(newPath)
        }
        this.pathsForAllPossibleAnswers = allAnswerPaths.map((path) => {
            const userJourney = new UserJourney(path, this);
            userJourney.setRecommendation(this.recommendation, path);
            return userJourney;
        });
    }

    checkAllChunksTested() {
        const sectionChunks = this.recommendation.section.chunks.map(chunk => chunk.id);

        const uniqueTestedChunks = [...Object.values(this.minimumPathsForRecommendations), ...this.pathsForAllPossibleAnswers.map(userJourney => userJourney.path)]
            .map(path => this.recommendation.section.getChunksForPath(path))
            .flat()
            .reduce((uniqueChunks, current) => {
                if (!uniqueChunks.includes(current.id)) {
                    uniqueChunks.push(current.id);
                }
                return uniqueChunks;
            }, [])
        
        if (sectionChunks.length !== uniqueTestedChunks.length) {
            sectionChunks.filter(chunkId => !uniqueTestedChunks.includes(chunkId)).forEach(missingChunk => {
                console.error(
                    `Recommendation chunk ${missingChunk} in ${this.name} not included in any test paths.`
                );
            })
        }
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
