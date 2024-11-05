import TestSuiteRow from "./test-suite-row.js";

const ADO_TAG = "Functional.js";

export default class StaticPageTests {
  testReferenceIndex = 1;

  testCaseGenerators = [
    this.generateAccessibilityPage.bind(this),
    this.generateContactUsPage.bind(this),
    this.generateCookiesPage.bind(this),
    this.generatePrivacyPolicyPage.bind(this),
    this.generate404Page.bind(this)
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

  generateContactUsPage() {
    const testScenario = `User can access the contact us page`;
    const testSteps = `1 - click the footer 'Contact us' link
    2 - Verify that the contact us page renders correctly`;
    const expectedOutcome = 'Contact information is rendered correctly';
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateCookiesPage() {
    const testScenario = `User can access the cookies page`;
    const testSteps = `1 - click the footer 'Cookies' link
    2 - Verify that the cookies page renders correctly`;
    const expectedOutcome = 'Cookie preference information is rendered correctly';
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generatePrivacyPolicyPage() {
    const testScenario = `User can access the privacy policy page`;
    const testSteps = `1 - click the footer 'Privacy Policy' link
    2 - Verify that you are taken to the correct privacy page`;
    const expectedOutcome =  'Privacy policy page is accessible';
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generate404Page() {
    const testScenario = `User can access a 404 page`;
    const testSteps = `1 - Navigate to a non-existent page (e.g. BASEURL/doesnt-exist)
    2 - Verify that the 404 page renders correctly`;
    const expectedOutcome = 'A 404 page is rendered';
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }
}
