export default class ContentError {
  id;
  contentType;
  message;

  constructor({ id, contentType, message }) {
    this.id = id;
    this.contentType = contentType;
    this.message = message;
  }
}
