import fs from "fs";
import { Recommendation } from "./recommendation.mjs";
import { Question } from "./question.mjs";
import { UserJourney } from "./user-journey.mjs";

const WriteAllPossiblePaths = false;

export class Section {
  recommendations;
  questions;
  name;
  id;

  paths;
  minimumPathsToNavigateQuestions;
  minimumPathsForRecommendations;

  constructor({ fields, sys }) {
    this.recommendations = fields.recommendations.map(
      (recommendation) => new Recommendation(recommendation)
    );

    this.questions = fields.questions.map((question) => new Question(question));
    this.id = sys.id;
    this.name = fields.name;

    this.setNextQuestions();
    this.paths = this.getAllPaths(this.questions[0]).map((path) => {
      const userJourney = new UserJourney(path, this);
      userJourney.setRecommendation(this.recommendations);

      return userJourney;
    });

    this.getMinimumPathsForQuestions();
    this.getMinimumPathsForRecommendations();
  }

  getMinimumPathsForQuestions() {
    const sortedPaths = this.paths
      .slice()
      .sort((a, b) => b.path.length - a.path.length);
    this.minimumPathsToNavigateQuestions = this.calculateMinimumPaths(
      sortedPaths,
      this.questions
    );
  }

  getMinimumPathsForRecommendations() {
    const pathsShortedByShortestFirst = this.paths
      .slice()
      .sort((a, b) => a.path.length - b.path.length);
    this.minimumPathsForRecommendations =
      this.calculateMinimumPathsForRecommendations(pathsShortedByShortestFirst);
  }

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

      return matchingPath.mapPathToOnlyQuestionAnswerTexts();
    });

    // If there are remaining questions, find paths for each question
    for (const questionId of remainingQuestions) {
      const pathContainingQuestion = this.getFirstPathContainingQuestion(
        sortedPaths,
        questionId
      );

      if (pathContainingQuestion == null) continue;

      minimumPaths.push(
        pathContainingQuestion.mapPathToOnlyQuestionAnswerTexts()
      );
    }

    return minimumPaths;
  }

  calculateMinimumPathsForRecommendations(paths) {
    const possibleMaturities = ["Low", "Medium", "High"];
    const minimumPathsForRecommendations = {};

    for (const maturity of possibleMaturities) {
      const pathForRecommendation = paths.find(
        (path) => path.maturity == maturity
      );

      if (!pathForRecommendation) {
        console.error(`No path found for ${maturity} in ${this.name}`);
        continue;
      }

      minimumPathsForRecommendations[maturity] =
        pathForRecommendation.mapPathToOnlyQuestionAnswerTexts();
    }

    return minimumPathsForRecommendations;
  }

  get stats() {
    return this.paths.reduce((count, path) => {
      const maturity = path.maturity;
      return count[maturity] ? ++count[maturity] : (count[maturity] = 1), count;
    }, {});
  }

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
          (q) => q.id === answer.nextQuestion?.id
        );

        stack.push({
          currentPath: newPath,
          currentQuestion: nextQuestion,
        });
      });
    }

    return paths;
  }

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
      console.error(`Couldn't find question ${questionId} in ${this.name}`);
      return;
    }
    console.error(
      `Unable to find a path containing question ${questionId} (${question.text}) in ${this.name}`
    );

    return;
  }

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
          console.log("");

          answer.nextQuestion = null;
          continue;
        }

        answer.nextQuestion = matchingQuestions[0];
      }
    }
  }

  writeFile(destinationFolder) {
    const output = {
      section: this.name,
      stats: this.stats,
      minimumQuestionPaths: this.minimumPathsToNavigateQuestions,
      minimumRecommendationPaths: this.minimumPathsForRecommendations,
      allPossiblePaths: WriteAllPossiblePaths
        ? this.paths.map((path) => {
            var result = {
              recommendation:
                path.recommendation != null
                  ? {
                      name: path.recommendation?.displayName,
                      maturity: path.recommendation?.maturity,
                    }
                  : null,
              path: path.path.map((s) => {
                return {
                  question: s.question.text,
                  answer: s.answer.text,
                };
              }),
            };

            return result;
          })
        : undefined,
    };

    const json = JSON.stringify(output);
    fs.writeFileSync(`${destinationFolder}/${this.name}.json`, json);
  }
}

const removeFromArray = (arr, value) => {
  var index = arr.indexOf(value);
  if (index > -1) {
    arr.splice(index, 1);
  }
  return arr;
};
