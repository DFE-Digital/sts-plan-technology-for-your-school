import MapContent from "#src/content-types/content-mapper";

export default class RecommendationIntro {
  content;
  header;
  id;
  maturity;
  slug;

  constructor({ fields, sys }) {
    this.content = MapContent(fields.content);
    this.header = fields.header.fields.text;
    this.id = sys.id;
    this.maturity = fields.maturity;
    this.slug = fields.slug;
  }
}