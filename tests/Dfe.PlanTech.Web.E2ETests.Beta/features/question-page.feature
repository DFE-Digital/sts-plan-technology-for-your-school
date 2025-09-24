@user-school
Feature: Questions Page

  Background:
    Given I start an assessment on category "Category One" section "Category One Section One"

  @clear-data-school
  Scenario: Page has the correct content on first load
    Then I should see the beta header
    And the header should contain all the correct content
    And I should see the GOV.UK footer with expected links
    And I should see a back link to the previous page
    And I should see the question heading "Are you able to choose an answer to this first question?"
    And I should see the question help text "First question help text"
    And I should see a continue button that submits to "/category-one/category-one-section-one/self-assessment/testing-question-one"
    And the question answer radio options should appear in this order:
      | Answer 1 |
      | Answer 2 |
      | Answer 3 |
    And no radio option should be selected

  Scenario: Selecting the answer enables the correct radio button
    When I choose the radio option "Answer 2"
    Then exactly one radio option should be selected and it should be "Answer 2"

  Scenario: Clicking the continue button progresses to the next question
    When I choose the radio option "Answer 2"
    And I click the continue button
    Then I should be on the URL containing "/category-one/category-one-section-one/self-assessment/testing-question-two"
    And I should see the question heading "Are you able to answer this second question?"
    And I should see the question help text "This is the second question help text"
    And the question answer radio options should appear in this order:
      | Answer One |
      | Answer 2 |
    And no radio option should be selected
