import fs from "fs";

const filename = "plan-tech-test-suite.csv";

const columns = [
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

const columnKeys = [];
const columnHeaders = [];

columns.forEach((columnObj) => {
  const [key, header] = Object.entries(columnObj)[0];
  columnKeys.push(key);
  columnHeaders.push(header);
});

export default function WriteCsv(testSuites) {
  let csv = "";
  csv += columnHeaders.join(",") + "\n";

  testSuites
    .flatMap((testSuite) => testSuite.testCases)
    .filter((testCase) => testCase != null)
    .map((testCase) =>
      Array.from(
        columnKeys.map((key) => {
          return testCase[key] ?? " ";
        })
      )
    )
    .forEach((row) => (csv += '"' + row.join('","') + '"\n'));

  fs.writeFileSync(filename, csv);
}
