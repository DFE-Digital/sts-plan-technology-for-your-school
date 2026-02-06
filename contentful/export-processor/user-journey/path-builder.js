import PathPart from './path-part.js';

export class PathBuilder {
  constructor(questions) {
    this.questions = questions;
  }

  /**
   * Retrieves all possible paths from the current question.
   */
  getPathsForQuestion(question) {
    const paths = [];
    const stack = [];

    stack.push({
      currentPath: [],
      currentQuestion: question,
    });

    while (stack.length > 0) {
      const { currentPath, currentQuestion } = stack.pop();

      if (!currentQuestion) {
        paths.push(currentPath);
        continue;
      }

      const questionPaths = currentQuestion.answers.map((answer) =>
        this._getPathsForAnswer(currentPath, currentQuestion, answer),
      );
      stack.push(...questionPaths);
    }

    return paths;
  }

  _getPathsForAnswer(currentPath, currentQuestion, answer) {
    const newPath = [...currentPath, new PathPart({ question: currentQuestion, answer: answer })];

    const nextQuestion = this.questions.find((q) => q.id === answer.nextQuestion?.sys.id);

    return {
      currentPath: newPath,
      currentQuestion: nextQuestion,
    };
  }

  /**
   * Selects an answer, prioritising shortening the overall path
   * @param {flattenedAnswer[]} answers
   * @param {number} index
   */
  assignBestAnswer(answers, index) {
    if (index === this.questions.length - 1) {
      return answers[0];
    }

    const nextQuestionInSection = this.questions[index + 1];

    const pathEnder = answers.find((answer) => !answer.nextQuestion);
    if (pathEnder) return pathEnder;

    const skipper =
      nextQuestionInSection &&
      answers.find(
        (answer) => answer.nextQuestion && answer.nextQuestion.id !== nextQuestionInSection.id,
      );

    if (skipper) return skipper;

    return answers.find(
      (answer) => answer.nextQuestion && answer.nextQuestion.id === nextQuestionInSection.id,
    );
  }
}

/**
 * @typedef {Object} flattenedAnswer
 * @property {string} id
 * @property {hasId} nextQuestion
 */

/**
 * @typedef {Object} hasId
 * @property {string} id
 */
