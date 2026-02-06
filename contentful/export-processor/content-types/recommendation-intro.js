export default class RecommendationIntro {
  content;
  header;
  id;
  maturity;
  slug;

  constructor({ fields, sys }) {
    this.content = fields.content;
    this.header = fields.header;
    this.id = sys.id;
    this.maturity = fields.maturity;
    this.slug = fields.slug;
  }
}
