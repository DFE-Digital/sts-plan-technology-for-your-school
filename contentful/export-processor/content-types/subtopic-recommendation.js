import RecommendationIntro from "./recommendation-intro.js";
import RecommendationSection from "./recommendation-section.js";
import { Section } from "./section.js";
import ErrorLogger from "../errors/error-logger.js";

export default class SubtopicRecommendation {
  intros;
  section;
  subtopic;
  id;

  constructor({ fields, sys }) {
    this.intros = fields.intros.map((recommendation) => new RecommendationIntro(recommendation));
    this.section = new RecommendationSection(fields.section);
    this.id = sys.id;

    this.subtopic = fields.subtopic && fields.subtopic.fields && fields.subtopic.sys ? new Section(fields.subtopic, this) : null;

    if (!this.subtopic) {
      ErrorLogger.addError({ id: sys.id, contentType: "subtopicRecommendation", message: `No Subtopic found` });
    }
  }

  getContentForMaturityAndPath({ maturity, path }) {
    const intro = this.intros.find((intro) => intro.maturity == maturity);

    const chunks = this.section.getChunksForPath(path);

    return {
      intro,
      chunks
    };
  }
}