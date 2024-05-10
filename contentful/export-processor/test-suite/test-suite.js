import TestSuiteRow from "#src/test-suite/test-suite-row";
//import Appendix from "#/src/test-suite/appendix";

const ADO_TAG = "Functional";

export default class TestSuiteForSubTopic {
  subtopic;
  testReferenceIndex;

  testCaseGenerators = [
    this.generateCanNavigateToSubtopic.bind(this),
    this.generateCanSaveAnswers.bind(this),
    this.generateAttemptsSaveWithoutAnswer.bind(this),
    this.generateSavesPartialAnswers.bind(this),
    this.generateReturnToPartialAnswers.bind(this),
    this.generateManualNavToURLQuestion.bind(this),
    this.generateCompleteJourney.bind(this),
    this.generateUpdateResubmitResponses.bind(this),
    this.generateReceivesLowScoringLogic.bind(this),
    this.generateReceivesHighScoringLogic.bind(this),
    this.generateReceivesMidScoringLogic.bind(this),
    this.generateChangeAnswersCheckYourAnswers.bind(this),
    this.generateReturnToSelfAssessment.bind(this),
  ];

  testCases = [];

  subtopicName;

  constructor({ subtopic, testReferenceIndex }) {
    this.subtopic = subtopic;
    this.subtopicName = this.subtopic.name.trim();
    this.testReferenceIndex = testReferenceIndex;

    this.testCases = this.testCaseGenerators.map((generator) => generator());
  }

  /*
    testReference,
    appendixRef,
    adoTag,
    subTopic,
    testScenario,
    preConditions,
    testSteps,
    expectedOutcome,
    testApproved,
*/
  createRow(testScenario, testSteps, expectedOutcome) {
    return new TestSuiteRow({
      testReference: `Access_${this.testReferenceIndex++}`,
      adoTag: ADO_TAG,
      subtopic: this.subtopicName,
      testScenario: testScenario,
      preConditions: "User is signed into the DfE Sign in service",
      testSteps: testSteps,
      expectedOutcome: expectedOutcome,
    });
  }

  generateCanNavigateToSubtopic() {
    const testScenario = `User can navigate to the ${this.subtopicName} subtopic from the self-assessment page`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic`;
    const expectedOutcome = `User sees ${this.subtopicName} interstitial page`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateCanSaveAnswers() {
    const testScenario = `User can save answers to questions for ${this.subtopicName} subtopic`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic 2 - Answer and save each question presented 3 - Compare the answers presented on 'Check your answers' page to your answers`;
    const expectedOutcome = `Answers match. Can save and continue.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateAttemptsSaveWithoutAnswer() {
    const testScenario = `User attempts to save and continue without selecting an answer`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic
                        2 - Do not answer question
                        3 - Select 'save and continue'`;
    const expectedOutcome = `User sees modal that states 'There is a problem. You must select an answer to continue'.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateSavesPartialAnswers() {
    const testScenario = `User saves partially completed ${this.subtopicName} subtopic answers`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic
                      2 - Select first answer
                      3 - Select back until reaching self assessment page`;
    const expectedOutcome = `User is returned to self assessment page. Broadband connection status shows 'In progress'.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateReturnToPartialAnswers() {
    const testScenario = `User can return to a partially completed ${this.subtopicName} question set and pick up where they left off`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic
                        2 - Navigate through the interstitial page`;
    const expectedOutcome = `User is returned to the first unanswered question. User can continue through to reach a recommendation.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateManualNavToURLQuestion() {
    const testScenario = `User manually goes to a question ahead of their journey for a subtopic via URL`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic
2 - Navigate ahead of journey via URL`;
    const expectedOutcome = `User journey shows in progress.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateCompleteJourney() {
    const testScenario = `User can complete journey through all questions in ${this.subtopicName} topic`;
    const testSteps = `1 - Navigate to ${this.subtopicName} subtopic
                        2 - Navigate through the interstitial page
                        3 - Navigate through questions/check answers
                        4 - Save and continue`;
    const expectedOutcome = `User returns to self-assessment page and sees success modal.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateUpdateResubmitResponses() {
    const testScenario = `A different user can update and re-submit their responses`;
    const testSteps = `1 - Navigate to the ${this.subtopicName} subtopic
                        2 - Navigate through the interstitial page
                        3 - Navigate through questions/check answers
                        4 - Save and continue`;
    const expectedOutcome = `Recommendations are updated.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateReceivesLowScoringLogic() {
    //TODO: use paths
    const testScenario = `User receives low scoring logic`;
    const testSteps = `1 - Navigate to the ${this.subtopicName} subtopic
                        2 - Navigate through the interstitial page
                        3 - STEPS GO HERE
                        4 - Save and continue
                        5 - View ${this.subtopicName} recommendation`;
    const expectedOutcome = `User taken to 'RECOMMENDATION PAGE SLUG' recommendation page.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateReceivesHighScoringLogic() {
    //TODO
  }

  generateReceivesMidScoringLogic() {
    //TODO
  }

  generateChangeAnswersCheckYourAnswers() {
    const testScenario = `User can change answers via the 'Check your answers' page`;
    const testSteps = `1 - Navigate to the ${this.subtopicName} subtopic
                        2 - Navigate through the interstitial page
                        3 - Navigate to the change button on 'Check your answers' page
                        4 - Change answer`;
    const expectedOutcome = `Users answers update to match new answers.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }

  generateReturnToSelfAssessment() {
    const testScenario = `User returns to self-assessment screen during question routing`;
    const testSteps = `1 - Navigate to the ${this.subtopicName} subtopic
                        2 - Navigate through the interstitial page
                        3 - Answer first question, save and continue
                        4 - User clicks PTFYS header`;
    const expectedOutcome = `User returned to self-assessment page. ${this.subtopicName} subtopic shows 'In progress'.`;
    return this.createRow(testScenario, testSteps, expectedOutcome);
  }
}
