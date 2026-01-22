const fs = require('fs');

const MUTATIONS_THRESHOLD = parseInt(process.env.MUTATIONS_THRESHOLD || process.argv[2] || '60');

const MAX_CHARACTER_COUNT = parseInt(process.env.MAX_CHARACTER_COUNT || process.argv[3] || '20000');

const INPUT_PATH = process.env.INPUT_PATH || process.argv[4];

const OUTPUT_PATH = process.env.OUTPUT_PATH || process.argv[5] || 'filtered-report-table.md';

/**
 * Removes columns from a row and returns an object with relevant information.
 * Only returns data for rows where the score is below the mutations threshold.
 *
 * @param {string} row - A string representing a single row of data, separated by "|".
 * @returns {Object|undefined} result - The processed row data, or undefined if score exceeds threshold
 * @returns {string} result.file - The name of the file from the first column
 * @returns {string} result.score - The numerical score as a string from the second column
 * @returns {string} result.survived - The survival status from the third column ('true' or 'false')
 * @example
 * // Input: "test.js | 0.75 | true"
 * // Output: { file: "test.js", score: "0.75", survived: "true" }
 *
 * // Input: "main.js | 0.95 | false"
 * // Output: undefined (if MUTATIONS_THRESHOLD < 0.95)
 */
const removeColumns = (row) => {
  const cols = row
    .split('|')
    .map((col) => col.trim())
    .filter(Boolean);
  const score = parseFloat(cols[1]);
  const survived = parseInt(cols[2]);

  // Only return data for rows where:
  //   - the score is below the mutations threshold and survived is greater than 0.
  //   - there is at least one survived (i.e. filter out entries where there is 0% coverage)
  if (score < MUTATIONS_THRESHOLD && survived > 0) {
    return {
      file: cols[0],
      score: score,
    };
  }
};

/**
 *
 * @param {string[]} columns
 * @returns {string} Columns joined, prefixed, and suffixed by pipes.
 */
const toTableRowString = (columns) => `| ${columns.join(' | ')} |`;

const isTableRow = (row) => row.indexOf('|') > -1;

/**
 *
 * @param {string} previous Table string combined so far
 * @param {string} current Current table row
 * @returns {string} Combined table string up to a certain max character count
 */
const combineLinesToLength = (previous, current) => {
  const currentLength = current.length;
  const previousLength = previous.length;

  if (currentLength + previousLength + 10 > MAX_CHARACTER_COUNT) {
    return previous;
  }

  return previous + '\n' + current;
};

/**
 *
 * @param {string} markdownContent
 * @returns {string}
 */
const filterTable = (markdownContent) => {
  const lines = markdownContent.split('\n');
  const headers = ['File', 'Score'];

  const tableHeaders = [headers, headers.map(() => ' - ')].map(toTableRowString).join('\n');

  const startOfTable = lines.findIndex(isTableRow);

  const contentStartingAtTable = lines.slice(startOfTable);
  const endOfTable = contentStartingAtTable.findIndex((row) => !isTableRow(row));

  const rowValues = contentStartingAtTable
    .slice(0, endOfTable + 1)
    .map(removeColumns)
    .filter((row) => !!row)
    .sort((a, b) => a.score - b.score)
    .map((row) => [row.file, `${row.score}%`])
    .map(toTableRowString);

  return [tableHeaders, ...rowValues].reduce(combineLinesToLength, '');
};

const readMutationsReport = (filePath) => {
  const content = fs.readFileSync(filePath, 'utf-8');

  return content;
};

const saveFilteredReport = (contents, filePath) => {
  fs.writeFileSync(filePath, contents);
};

const processMutationsReport = () => {
  const content = readMutationsReport(INPUT_PATH);

  const filtered = filterTable(content);
  saveFilteredReport(filtered, OUTPUT_PATH);
};

processMutationsReport();
