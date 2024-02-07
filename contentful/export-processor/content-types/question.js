import { Answer } from "./answer.js";

export class Question {
  answers;
  text;
  helpText;
  slug;
  id;

  constructor({ fields, sys }) {
    this.answers = fields.answers.map((answer) => new Answer(answer));
    this.text = fields.text;
    this.helpText = fields.helpText;
    this.slug = fields.slug;
    this.id = sys.id;
  }
}
