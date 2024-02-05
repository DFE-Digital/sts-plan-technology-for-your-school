import { Answer } from "./answer.js";

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
