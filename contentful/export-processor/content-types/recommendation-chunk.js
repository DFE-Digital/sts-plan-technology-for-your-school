import { Answer } from "#src/content-types/answer";
import ErrorLogger from "#src/errors/error-logger";
import MapContent from "#src/content-types/content-mapper";

export default class RecommendationChunk {
  title;
  header;
  content;
  answers;
  id;

  constructor({ fields, sys }) {
    this.title = fields.title;
    this.header = fields.header.fields.text;
    this.id = sys.id;
    this.content = MapContent(fields.content);
    this.answers = fields.answers?.map(answer => new Answer(answer)) ?? [];

    if (!this.answers || this.answers.length == 0) {
      ErrorLogger.addError({ id: sys.id, contentType: "recommendationChunk", message: `No answers for chunk` });
    }
  }
}