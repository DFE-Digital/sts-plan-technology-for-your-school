import { Answer } from "#src/content-types/answer";
import RecommendationChunk from "#src/content-types/recommendation-chunk";

export default class RecommendationSection {
  answers;
  chunks;
  id;

  constructor({ fields, sys }) {
    this.answers = fields.answers?.map((answer) => {
      return new Answer(answer);
    }) ?? [];

    if (this.answers.length == 0) {
      console.log(`No answers for recommendation section ${sys.id}`);
    }

    this.chunks = fields.chunks?.map((chunk) => new RecommendationChunk(chunk)) ?? [];
    if (this.chunks.length == 0) {
      console.log(`No chunks for recommendation section ${sys.id}`);
    }

    this.id = sys.id;
  }
}