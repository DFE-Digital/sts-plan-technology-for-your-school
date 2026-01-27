import 'dotenv/config';
import TestSuite from '#src/test-suite/test-suite';
import writeTestSuites from './test-suite/write-csvs.js';
import StaticPageTests from './test-suite/static-page-tests.js';
import { log } from './helpers/log.js';

/**
 * @param {object} args
 * @param {import("./data-mapper.js").default} args.dataMapper - created and initialised DataMapper
 * @param {string} args.outputDir - directory to write journey paths to
 */
export default function generateTestSuites({ dataMapper, outputDir }) {
  let index = 1;
  const staticPageTests = new StaticPageTests();

  log('Generating test suites');

  const testSuites = Object.values(dataMapper.mappedSections)
    .filter((section) => !!section)
    .map((section) => {
      log(`Generating test suite for ${section.name}`);

      const testSuite = new TestSuite({
        subtopic: section,
        testReferenceIndex: index,
      });
      index = testSuite.testReferenceIndex;

      log(`Test suite for ${section.name} generated`, {
        addSeperator: true,
      });

      return testSuite;
    });

  log('Generated test suites');

  testSuites.push(staticPageTests);
  writeTestSuites({ testSuites, outputDir });

  log('Wrote test suite CSVs', { addSeperator: true });
}
