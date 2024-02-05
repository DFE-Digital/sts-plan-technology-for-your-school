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
