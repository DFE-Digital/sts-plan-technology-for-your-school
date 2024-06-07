import { Answer } from "#src/content-types/answer";
import RecommendationChunk from "#src/content-types/recommendation-chunk";
import ErrorLogger from "#src/errors/error-logger";

export default class RecommendationSection {
  answers;
  chunks;
  id;

  constructor({ fields, sys }) {
    this.id = sys.id;

    this.answers = fields.answers?.map((answer) => new Answer(answer)) ?? [];

    this.chunks = fields.chunks?.map((chunk) => new RecommendationChunk(chunk)) ?? [];
    this.logErrorIfMissingRelationships("chunks");
  }

  logErrorIfMissingRelationships(field) {
    const matching = this[field];
    if (!matching || matching.length == 0) {
      ErrorLogger.addError({ id: this.id, contentType: "recommendationSection", message: `No ${field} found` });
    }
  }

  getChunksForPath(path) {
    return path.map(pathPart => pathPart.answer.id).flatMap(answerId => this.chunks.filter(chunk => chunk.answers.some(answer => answer.id == answerId)));
  }
}