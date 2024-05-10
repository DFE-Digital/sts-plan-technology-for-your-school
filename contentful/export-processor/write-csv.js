import fs from "fs";

const mainSheetColumns = [
  { testReference: "Test Reference" },
  { appendixRef: "Appendix Reference" },
  { adoTag: "ADO Tag" },
  { subtopic: "Sub-topic" },
  { testScenario: "Test Scenario" },
  { preConditions: "Pre-conditions" },
  { testSteps: "Test Steps" },
  { expectedOutcome: "Expected Outcome" },
  { testApproved: "Test Approved" },
];

const mainSheetKeys = [];
const mainSheetHeaders = [];

mainSheetColumns.forEach((columnObj) => {
  const [key, header] = Object.entries(columnObj)[0];
  mainSheetKeys.push(key);
  mainSheetHeaders.push(header);
});

const appendixColumns = [
  { reference: "Reference" },
  { content: "Content" },
]
const appendixSheetKeys = [];
const appendixSheetHeaders = [];

appendixColumns.forEach((columnObj) => {
  const [key, header] = Object.entries(columnObj)[0];
  appendixSheetKeys.push(key);
  appendixSheetHeaders.push(header);
});

export default function WriteCsv(testSuites) {
  writeSheet(testSuites.flatMap(suite => suite.testCases), mainSheetHeaders, mainSheetKeys, "plan-tech-test-suite.csv");
  writeSheet(testSuites.flatMap(suite => suite.appendix), appendixSheetHeaders, appendixSheetKeys, "plan-tech-test-suite-appendix.csv");
}

function writeSheet(content, headers, keys, filename) {
  let csv = "";
  csv += headers.join(",") + "\n";

  content
    .filter((testCase) => testCase != null)
    .map((testCase) => Array.from(
      keys.map((key) => {
        return testCase[key] ?? " ";
      })
    )
    )
    .forEach((row) => (csv += '"' + row.join('","') + '"\n'));

  fs.writeFileSync(filename, csv);
}

