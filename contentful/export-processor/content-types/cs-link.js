export default class CSLink {
  url;
  linkText;

  constructor({ fields }) {
    this.url = fields.url;
    this.linkText = fields.linkText;
  }
}
