export class Section {
  recommendations;
  questions;
  id;

  constructor({ fields, sys }) {
    console.log(fields);
    this.recommendations = fields.recommendations.map((recommendation) => {
      console.log(recommendation);
      return new Recommendation(recommendation);
    });
    this.questions = fields.questions.map((question) => new Question(question));
    this.id = sys.id;
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
    this.id = sys.id;
  }
}

export class Answer {
  id;
  text;
  maturity;

  constructor({ fields, sys }) {
    this.maturity = fields.maturity;
    this.text = fields.text;
    this.id = sys.id;
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
