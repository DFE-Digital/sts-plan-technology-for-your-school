import RecommendationIntro from "#src/content-types/recommendation-intro";
import RecommendationSection from "#src/content-types/recommendation-section";
import { Section } from "#src/content-types/section";

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
      console.log(`No subtopic for subtopic recommendation ${sys.id}`);
    }
  }
}