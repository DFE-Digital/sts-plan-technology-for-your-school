export class Section {
  recommendations;
  questions;
  id;

  paths;

  constructor({ fields, sys }) {
    this.recommendations = fields.recommendations.map((recommendation) => {
      return new Recommendation(recommendation);
    });
    this.questions = fields.questions.map((question) => new Question(question));
    this.id = sys.id;

    this.setNextQuestions();
    this.paths = [];
    this.paths = this.getAllPaths(this.questions[0]);
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
          { question: currentQuestion.text, answer: answer.text },
        ];

        const nextQuestion = this.questions.find(
          (q) => q.id === answer.nextQuestion?.id
        );
        /*
        console.log(
          "next question for answer",
          answer,
          answer.nextQuestion?.Id,
          nextQuestion
        );
        */
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
            `Error finding question for ${nextQuestionId} in ${this.name}`
          );
        }
        answer.nextQuestion = matchingQuestions[0];
      }
    }
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
  name;
  id;

  constructor({ fields, sys }) {
    this.slug = fields.slug;
    this.maturity = fields.maturity;
    this.name = fields.name;
    this.id = sys.id;
  }
}

export class UserJourney {
  path;

  constructor() {
    this.path = [];
  }
}
