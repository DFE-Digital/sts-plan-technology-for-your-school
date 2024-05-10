import RecommendationIntro from "#src/content-types/recommendation-intro";
import RecommendationSection from "#src/content-types/recommendation-section";
import { Section } from "#src/content-types/section";

export default class SubtopicRecommendation {
  intros;
  section;
  subtopic;
  id;

  constructor({ fields, sys }) {
    this.intros = fields.intros.map((recommendation) => {
      return new RecommendationIntro(recommendation);
    });

    this.subtopic = new Section(fields.section);
    this.section = new RecommendationSection(fields.section);
    this.id = sys.id;
  }
}