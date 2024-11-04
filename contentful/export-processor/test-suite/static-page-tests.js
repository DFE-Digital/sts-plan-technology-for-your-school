import TestSuiteRow from "./test-suite-row.js";

const ADO_TAG = "Functional.js";

export default class StaticPageTests {
  testReferenceIndex = 1;

  testCaseGenerators = [
    this.generateAccessibilityPage.bind(this),
  ];

  testCases = [];
  appendix = [];
  subtopicName;

  constructor() {
    this.testCases = this.testCaseGenerators.map((generator) => generator()).filter((testCase) => !!testCase);
  }

  createRow(testScenario, testSteps, expectedOutcome, appendix) {
    const row = new TestSuiteRow({
      testReference: `Static_${this.testReferenceIndex++}`,
      adoTag: ADO_TAG,
      subtopic: null,
      testScenario: testScenario,
      preConditions: "None",
      testSteps: testSteps,
      expectedOutcome: expectedOutcome,
      appendixRef: appendix?.reference
    });
    return row;
  }

  generateAccessibilityPage() {
    const testScenario = `User can access the accessibility page`;
    const testSteps = `1 - click the footer 'Accessibility' link
    2 - Verify that the accessibility page renders correctly`;
    const expectedOutcome = 'Accessility information is rendered correctly';
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }
}
