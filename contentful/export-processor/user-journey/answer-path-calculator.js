import PathPart from './path-part.js';
import { UserJourney } from './user-journey.js';

export class AnswerPathCalculator {
  constructor(questions, pathBuilder) {
    this.questions = questions;
    this.pathBuilder = pathBuilder;
  }

  /**
   *
   * @param {Record<string, PathPart[]>} minimumPathsForRecommendations
   * @returns
   */
  getPathsForAllAnswers(minimumPathsForRecommendations) {
    const allAnswers = this._getAnswerIdsFromQuestions(this.questions);
    const answersUsed = this._getUsedAnswerIds(minimumPathsForRecommendations);
    const allAnswerPaths = [];

    while (!this._allAnswersUsed(allAnswers, answersUsed)) {
      const startIndex = this._findLastQuestionWithUnusedAnswers(answersUsed);
      if (startIndex === -1) break;

      const newPath = this._buildPathFromIndex(startIndex, answersUsed);
      allAnswerPaths.push(newPath);

      this._updateUsedAnswers(newPath, answersUsed);
    }

    return allAnswerPaths;
  }

  /**
   * Selects an answer, prioritising shortening the overall path (by favouring answers that end the path early or skip later questions).
   *
   * @param {Array<Answer>} answers - answers to select from: unused answers (if called with the last question in the section with unusued answers) or all answers.
   * @param {number} index - index of current question.
   * @return {Answer} - selected answer.
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

  /**
   *
   * @param {Iterable<import("../content-types/question.js").Question>} questions
   * @returns {Iterable<string>}
   */
  _getAnswerIdsFromQuestions(questions) {
    return questions.flatMap((question) => question.answerIds);
  }

  /**
   * @param {Map<string, UserJourney>} minimumPathsForRecommendations
   */
  _getUsedAnswerIds(minimumPathsForRecommendations) {
    return [
      ...new Set(
        Object.values(minimumPathsForRecommendations)
          .flat()
          .map((part) => part.answer.id),
      ),
    ];
  }

  /**
   *
   * @param {import("../content-types/answer.js")[]} answersUsed
   */
  _allAnswersUsed(allAnswers, answersUsed) {
    return answersUsed.length === allAnswers.length;
  }

  /**
   *
   * @param {Set<import("../content-types/answer.js").Answer>} answersUsed
   */

  _findLastQuestionWithUnusedAnswers(answersUsed) {
    return this.questions.findLastIndex((question) =>
      question.answers.some((answer) => !answersUsed.includes(answer.id)),
    );
  }

  /**
   *
   * @param {number} startIndex
   * @param {Set<import("../content-types/answer.js").Answer>} answersUsed
   */
  _buildPathFromIndex(startIndex, answersUsed) {
    let newPath = [];
    const lastQuestionWithUnusedAnswers = this.questions[startIndex];

    // Initialize path with the last unused answer
    const { latestAnswer, nextNeeded } = this._initializePathStart(
      lastQuestionWithUnusedAnswers,
      answersUsed,
    );

    newPath.unshift({
      question: lastQuestionWithUnusedAnswers,
      answer: latestAnswer,
    });

    this._buildBackwardsPath(newPath, startIndex, nextNeeded, answersUsed);
    this._buildForwardsPath(newPath, latestAnswer);

    return newPath;
  }

  /**
   *
   * @param {import("../content-types/question.js").Question} question
   * @param {Set<import("../content-types/answer.js").Answer>} answersUsed
   */
  _initializePathStart(question, answersUsed) {
    const lastUnusedAnswers = question.answers.filter((answer) => !answersUsed.includes(answer.id));
    const latestAnswer = this.assignBestAnswer(lastUnusedAnswers, this.questions.indexOf(question));

    return {
      latestAnswer,
      nextNeeded: question.id,
    };
  }

  /**
   *
   * @param {import("./path-part.js")[]} path
   * @param {number} startIndex
   * @param {import("../content-types/question.js").Question} nextNeeded
   * @param {Set<import("../content-types/answer.js").Answer>} answersUsed
   */
  _buildBackwardsPath(path, startIndex, nextNeeded, answersUsed) {
    for (let i = startIndex - 1; i >= 0; i--) {
      const currentQuestion = this.questions[i];
      const answer = this._findBestPreviousAnswer(currentQuestion, nextNeeded, answersUsed);

      if (answer) {
        nextNeeded = currentQuestion.id;
        path.unshift({
          question: currentQuestion,
          answer: answer,
        });
      }
    }
  }

  /**
   *
   * @param {import("./path-part.js")[]} path
   * @param {import("../content-types/question.js").Question} nextNeeded
   * @param {Set<import("../content-types/answer.js").Answer>} answersUsed
   */
  _findBestPreviousAnswer(question, nextNeeded, answersUsed) {
    const unusedAnswers = question.answers.filter(
      (answer) => !answersUsed.includes(answer.id) && !!answer.nextQuestion,
    );

    if (unusedAnswers.some((answer) => answer.nextQuestion.id === nextNeeded)) {
      return unusedAnswers.find((answer) => answer.nextQuestion.id === nextNeeded);
    }

    return question.answers.find((answer) => answer.nextQuestion?.id === nextNeeded);
  }

  /**
   *
   * @param {import("./path-part.js")[]} path
   * @param {Set<import("../content-types/answer.js").Answer>} answersUsed
   */
  _buildForwardsPath(path, latestAnswer) {
    let currentAnswer = latestAnswer;
    while (currentAnswer?.nextQuestion) {
      const nextIndex = this.questions.findLastIndex(
        (question) => question.id === currentAnswer.nextQuestion.id,
      );
      const nextQuestion = this.questions[nextIndex];
      const nextAnswer = this.assignBestAnswer(nextQuestion.answers, nextIndex);

      path.push({ question: nextQuestion, answer: nextAnswer });
      currentAnswer = nextAnswer;
    }
  }

  /**
   *
   * @param {UserJourney} path
   * @param {Set<string>} answersUsed
   */
  _updateUsedAnswers(path, answersUsed) {
    path.forEach((pathPart) => {
      if (!answersUsed.includes(pathPart.answer.id)) {
        answersUsed.push(pathPart.answer.id);
      }
    });
  }
}
