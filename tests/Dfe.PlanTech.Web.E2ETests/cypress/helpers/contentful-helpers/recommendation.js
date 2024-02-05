export class Recommendation {
  slug;
  maturity;
  displayName;
  id;
  page;

  constructor({ fields, sys }) {
    this.slug = fields.slug;
    this.maturity = fields.maturity;
    this.displayName = fields.displayName;
    this.id = sys.id;
    this.page = fields.page;
    this.slug = fields.page?.fields.slug;
  }
}
