import fs from "fs";
export class Section {
  recommendations;
  questions;
  name;
  id;

  paths;

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
      paths: this.paths.map((path) => {
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
      }),
    };

    const json = JSON.stringify(output);
    fs.writeFileSync(`${destinationFolder}/${this.name}.json`, json);
  }
}

export class Question {
  answers;
  text;
  slug;
  id;

  constructor({ fields, sys }) {
    this.answers = fields.answers.map((answer) => new Answer(answer));
    this.text = fields.text;
    this.slug = fields.slug;
    this.id = sys.id;
  }
}

export class Answer {
  id;
  text;
  maturity;
  nextQuestion;

  constructor({ fields, sys }) {
    this.maturity = fields.maturity;
    this.text = fields.text;
    this.id = sys.id;
    this.nextQuestion = fields.nextQuestion;
  }
}

export class Recommendation {
  slug;
  maturity;
  displayName;
  id;
  page;

  constructor({ fields, sys }) {
    this.slug = fields.slug;
    this.maturity = fields.maturity;
    this.displayName = fields.displayName;
    this.id = sys.id;
    this.page = fields.page;
    this.slug = fields.page?.fields.slug;
  }
}

export class QuestionAnswer {
  question;
  answer;

  constructor({ question, answer }) {
    this.question = question;
    this.answer = answer;
  }
}

export class UserJourney {
  path;
  maturity;
  section;
  recommendation;

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
  }

  setRecommendation(recommendations) {
    const recommendation = recommendations.filter(
      (recommendation) => recommendation.maturity == this.maturity
    );

    if (recommendation == null || recommendation.length == 0) {
      console.error(
        `could not find recommendation for ${this.maturity} in ${this.section.name}`,
        recommendations
      );
      return;
    }

    this.recommendation = recommendation[0];
  }

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
}

const onlyUnique = (value, index, array) => array.indexOf(value) === index;
