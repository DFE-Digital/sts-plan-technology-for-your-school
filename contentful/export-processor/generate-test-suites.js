import "dotenv/config";
import TestSuite from "#src/test-suite/test-suite";
import writeTestSuites from "./test-suite/write-csvs.js";

/**
 * @param {object} args
 * @param {DataMapper} args.dataMapper - created and initialised DataMapper
 * @param {string} args.outputDir - directory to write journey paths to
 */
export default function generateTestSuites({ dataMapper, outputDir }) {
  let index = 1;

  const testSuites = Object.values(dataMapper.mappedSections)
    .filter(section => !!section)
    .map((section) => {
      const testSuite = new TestSuite({
        subtopic: section,
        testReferenceIndex: index,
      });

      index = testSuite.testReferenceIndex;

      return testSuite;
    });

  writeTestSuites({ testSuites, outputDir });
}