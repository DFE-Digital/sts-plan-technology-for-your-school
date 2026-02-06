import fs from 'fs';
import path from 'path';
// eslint-disable-next-line no-unused-vars
import TestSuite from './test-suite.js';
import { log } from '#src/helpers/log';

const mainSheetColumns = [
  { testReference: 'Test Reference' },
  { appendixRef: 'Appendix Reference' },
  { adoTag: 'ADO Tag' },
  { subtopic: 'Sub-topic' },
  { testScenario: 'Test Scenario' },
  { preConditions: 'Pre-conditions' },
  { testSteps: 'Test Steps' },
  { expectedOutcome: 'Expected Outcome' },
  { testApproved: 'Test Approved' },
];

const appendixColumns = [{ reference: 'Reference' }, { content: 'Content' }];

const createCsvInfo = (columns) => {
  const keys = [];
  const headers = [];

  columns.forEach((columnObj) => {
    const [key, header] = Object.entries(columnObj)[0];
    keys.push(key);
    headers.push(header);
  });

  return {
    headers: headers,
    keys: keys,
  };
};

const createCsvs = () => {
  const mainCsv = createCsvInfo(mainSheetColumns);
  const appendixCsv = createCsvInfo(appendixColumns);

  return { mainCsv, appendixCsv };
};

/**
 *
 * @param {*} param0
 * @param {TestSuite[]} param0.testSuites - test suites to save as CSV
 * @param {string} param0.outputDir - where to save files to
 */
export default function writeTestSuites({ testSuites, outputDir }) {
  log(`Creating CSVs for test suites`);
  const { mainCsv, appendixCsv } = createCsvs();

  outputDir = createTestSuitesSubDirectory(outputDir);

  writeSheet({
    content: testSuites.flatMap((suite) => suite.testCases),
    outputDir,
    fileName: 'plan-tech-test-suite.csv',
    ...mainCsv,
  });

  writeSheet({
    content: testSuites.flatMap((suite) => suite.appendix),
    outputDir,
    fileName: 'plan-tech-test-suite-appendix.csv',
    ...appendixCsv,
  });
}

const createTestSuitesSubDirectory = (outputDir) => {
  const subDirectory = path.join(outputDir, 'test-suites');

  try {
    fs.mkdirSync(subDirectory, { recursive: true });
  } catch (e) {
    console.error(`Error creating test-suites subdirectory`, e);
    return outputDir;
  }

  return subDirectory;
};

const writeSheet = ({ content, headers, keys, fileName, outputDir }) => {
  log(`Writing sheet for ${fileName}`);
  const rows = content
    .filter((testCase) => testCase != null)
    .map((testCase) => `"${keys.map((key) => testCase[key] ?? ' ').join(`","`)}"`)
    .join('\n');

  const headerRow = headers.join(',');
  const csv = `${headerRow}\n${rows}`;

  const filePath = path.join(outputDir, fileName);

  fs.writeFileSync(filePath, csv);
  log(`Wrote for ${fileName}`);
};
