import { Answer } from "#src/content-types/answer";

export default class RecommendationChunk {
  title;
  header;
  content;
  answers;
  id;

  constructor({ fields, sys }) {
    this.title = fields.title;
    this.header = fields.header;
    this.id = sys.id;
    this.content = fields.content;
    this.answers = fields.answers.map(answer => new Answer(answer));
  }
}