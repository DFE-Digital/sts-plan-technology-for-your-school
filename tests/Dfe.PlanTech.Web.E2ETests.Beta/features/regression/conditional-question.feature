@serial
@user-school
@clear-data-school
Feature: Conditional questions

  Background:
    Given I start an assessment on category "Category Two" section "Category Two Section Three"
  @clear-data-school
  Scenario: Answering question does not show the conditional question
    When I choose the radio option "Yes"
    And I click the continue button
    When I choose the radio option "Carrot"
    And I click the continue button
    Then I should see the question heading 'Which of these are known as the "Red planet"?'
    And I should see the question help text "Category two section three question three help text"
    And I should see a continue button that submits to "/category-two/category-two-section-three/self-assessment/testing-category-two-section-three-question-three"
    And the question answer radio options should appear in this order:
      | Earth |
      | Mars |
      | Jupiter |
      | Saturn |
    And no radio option should be selected

  @clear-data-school
  Scenario: Answering question shows the conditional question and continues the path
    When I choose the radio option "Yes"
    And I click the continue button
    When I choose the radio option "Apple"
    And I click the continue button
    Then I should see the question heading 'What colour can an apple be?'
    And I should see the question help text "Category two section three question four help text"
    And I should see a continue button that submits to "/category-two/category-two-section-three/self-assessment/testing-category-two-section-three-question-four"
    And the question answer radio options should appear in this order:
      | Green |
      | Blue |
      | Yellow |
    And no radio option should be selected
    When I choose the radio option "Green"
    And I click the continue button
    Then I should see the question heading 'Which of these are known as the "Red planet"?'
