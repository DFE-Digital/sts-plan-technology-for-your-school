import { Answer } from "./answer.js";
import RecommendationChunk from "./recommendation-chunk.js";
import ErrorLogger from "../errors/error-logger.js";

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
      const answerIds = path.map(pathPart => pathPart.answer.id);

      const filteredChunks = this.chunks.filter(chunk =>
          chunk.answers.some(answer => answerIds.includes(answer.id))
      );

      const uniqueChunks = [];
      const seen = new Set();

      for (const chunk of filteredChunks) {
          const chunkStr = JSON.stringify(chunk);
          if (!seen.has(chunkStr)) {
              seen.add(chunkStr);
              uniqueChunks.push(chunk);
          }
      }
      return uniqueChunks;
  }
}