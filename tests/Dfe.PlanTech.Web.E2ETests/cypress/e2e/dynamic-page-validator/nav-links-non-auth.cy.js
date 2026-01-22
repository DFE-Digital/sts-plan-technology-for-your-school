import DataMapper from 'export-processor/data-mapper.js';
import { validateNavigationLinks, validateAndTestNonAuthorisedPages } from './validators/index.js';

const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe('Navigation links and non-authorised pages', () => {
  it('Should render navigation links', () => {
    validateNavigationLinks(dataMapper);
  });

  validateAndTestNonAuthorisedPages(dataMapper);
});
