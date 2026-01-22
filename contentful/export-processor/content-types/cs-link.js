export default class CSLink {
  id;
  url;
  linkText;

  constructor({ fields, sys }) {
    this.id = sys.id;
    this.url = fields.url;
    this.linkText = fields.linkText;
  }
}
