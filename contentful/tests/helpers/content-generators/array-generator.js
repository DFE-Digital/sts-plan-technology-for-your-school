import { LinkGenerator } from './link-generator.js';
import { randomRange } from '../helpers.js';
export class ArrayGenerator extends LinkGenerator {
  constructor() {
    super('Array');
  }

  concreteGenerateContent(field) {
    return randomRange({ min: 1, max: 10, callback: this.generateEntryLink });
  }

  addIds(value, field) {}

  postProcess(value, field, groupedContent) {
    if (!value) {
      console.log('No value');
      return;
    }

    const items = value['en-US'];
    const acceptedContentTypes = this.acceptedContentTypes(field.items);

    if (!acceptedContentTypes || acceptedContentTypes.length == 0) {
      console.error(`Could not get any accepted content typers for field`, field);
      return;
    }

    for (const item of items) {
      this.setIdToAcceptedContent(item, groupedContent, acceptedContentTypes);
    }
  }

  acceptedContentTypesForField(field) {}
}
