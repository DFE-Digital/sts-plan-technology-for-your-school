export default class TestSuiteRow {
  testReference;
  appendixRef;
  adoTag;
  subtopic;
  testScenario;
  preConditions;
  testSteps;
  expectedOutcome;
  testApproved;

  constructor({
    testReference,
    appendixRef,
    adoTag,
    subtopic,
    testScenario,
    preConditions,
    testSteps,
    expectedOutcome,
    testApproved,
  }) {
    this.testReference = testReference;
    this.appendixRef = appendixRef;
    this.adoTag = adoTag;
    this.subtopic = subtopic;
    this.testScenario = testScenario;
    this.preConditions = preConditions;
    this.testSteps = testSteps;
    this.expectedOutcome = expectedOutcome;
    this.testApproved = testApproved;
  }
}
