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
        const allAnswerPaths = [];

        // Log ids of all answers in section
        const allAnswers = this.questions.flatMap(question => question.answers.map(answer => answer.id));

        // Log ids of answers used in minimum paths for recommendations
        const allRecommendationPaths = this.minimumPathsForRecommendations;
        const answersUsed = Object.values(allRecommendationPaths).flat().reduce((previous, current) => {
            if (previous.indexOf(current.answer.id) === -1) {
                previous.push(current.answer.id);
            }
            return previous;
        }, []);

        // Create paths until all answers are used
        while (answersUsed.length !== allAnswers.length) {
            let newPath = [];
            const startIndex = this.questions.findLastIndex(question => question.answers.some(answer => !answersUsed.includes(answer.id)));

            if (startIndex === -1) {
                console.log("All answers have been used.");
                break;
            }

            // Find the latest question in the sequence that has unused answers and assign an unused answer
            const lastQuestionWithUnusedAnswers = this.questions[startIndex];
            const lastUnusedAnswers = lastQuestionWithUnusedAnswers.answers.filter(answer => !answersUsed.includes(answer.id));
            let latestAnswer = this.assignBestAnswer(lastUnusedAnswers, startIndex);
            let nextNeeded = lastQuestionWithUnusedAnswers.id;

            newPath.unshift({ question: lastQuestionWithUnusedAnswers, answer: latestAnswer });

            // Loop backwards through questions leading up to the last with unused answers, adding answers that lead to needed questions
            for (let i = startIndex - 1; i >= 0; i--) {
                const currentQuestion = this.questions[i];
                let currentAnswer;
                const unusedAnswers = currentQuestion.answers.filter(answer => !answersUsed.includes(answer.id));

                // Find answers that lead to the next question in the sequence, prioritising unused answers and skipping any that jump ahead
                if (unusedAnswers.some(answer => answer.nextQuestion?.sys.id === nextNeeded)) {
                    currentAnswer = unusedAnswers.find(answer => answer.nextQuestion.sys.id === nextNeeded);
                } else if (currentQuestion.answers.some(answer => answer.nextQuestion.sys.id === nextNeeded)) {
                    currentAnswer = currentQuestion.answers.find(answer => answer.nextQuestion.sys.id === nextNeeded);
                } else {
                    console.log(`No valid answer found for question ${currentQuestion.id}. Skipping.`);
                }

                if (currentAnswer) {
                    nextNeeded = currentQuestion.id;
                    newPath.unshift({ question: currentQuestion, answer: currentAnswer });
                }
            }

            // Loop forwards from the last question with unused answers, adding answers to complete the sequence, prioritising ending early or skipping questions
            while (latestAnswer && latestAnswer.nextQuestion) {
                const nextIndex = this.questions.findLastIndex(question => question.id === latestAnswer.nextQuestion.sys.id);
                const nextQuestion = this.questions[nextIndex];
                const nextAnswer = this.assignBestAnswer(nextQuestion.answers, nextIndex);
                newPath.push({ question: nextQuestion, answer: nextAnswer });
                latestAnswer = nextAnswer;
            }

            allAnswerPaths.push(newPath);

            // Add answers to answersUsed
            newPath.forEach(pathPart => {
                if (!answersUsed.includes(pathPart.answer.id)) {
                    answersUsed.push(pathPart.answer.id);
                }
            })
        }

        this.pathsForAllPossibleAnswers = allAnswerPaths.map((path) => {
            const userJourney = new UserJourney(path, this);
            userJourney.setRecommendation(this.recommendation);
            return userJourney;
        });
    }

    /**
     * Selects an answer, prioritising shortening the overall path (by favouring answers that end the path early or skip later questions).
     *
     * @param {array} answers - available answers for current question.
     * @param {array} index - index of current question.
     * @return {Answer} - selected answer.
     */
    assignBestAnswer(answers, index) {
        if (index === this.questions.length - 1) {
            return answers[0];
        }

        const nextQuestionInSection = this.questions[index + 1];

        const pathEnder = answers.find(answer => !answer.nextQuestion);
        if (pathEnder) return pathEnder;

        const skipper = nextQuestionInSection && answers.find(answer => answer.nextQuestion && answer.nextQuestion.id !== nextQuestionInSection.id);
        if (skipper) return skipper;

        return answers.find(answer => answer.nextQuestion && answer.nextQuestion.id === nextQuestionInSection.id);

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
                console.error(`Recommendation chunk ${missingChunk} in ${this.name} not included in any test paths.`);
            });
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
