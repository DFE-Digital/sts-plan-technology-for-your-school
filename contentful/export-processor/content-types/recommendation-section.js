import { Answer } from "#src/content-types/answer";
import RecommendationChunk from "#src/content-types/recommendation-chunk";

export default class RecommendationSection {
  answers;
  chunks;
  id;

  constructor({ fields, sys }) {
    this.answers = fields.answers.map((answer) => {
      return new Answer(answer);
    });

    this.chunks = fields.chunks.map((chunk) => new RecommendationChunk(chunk));
    this.id = sys.id;
  }
}