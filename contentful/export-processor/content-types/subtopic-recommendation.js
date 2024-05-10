import RecommendationIntro from "#src/content-types/recommendation-intro";
import RecommendationSection from "#src/content-types/recommendation-section";
import { Section } from "#src/content-types/section";
import ErrorLogger from "#src/errors/error-logger";

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
    }
  }
}