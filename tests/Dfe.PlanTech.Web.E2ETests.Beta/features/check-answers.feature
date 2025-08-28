Feature: Check answers page
  @clear-data-school
  Scenario: Check answers page has all the correct elements
    Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1" and I do not click submit recommendations

    Then I should see the page heading "Check your answers"
    And the header should contain all the correct content
    And I should see the section heading "Category One Section One"

    And I should see a print button
    And I should see a view recommendations button
    And I should see a back link to the previous page
    And I should see the GOV.UK footer with expected links
    And I should see the beta header
    And the page should be accessible 

    And I should see the following ordered check answer rows:
        | Question                                                   | Answer     | Href                                            |
        | Are you able to choose an answer to this first question?   | Answer 1   | /category-one/category-one-section-one/self-assessment/testing-question-one  |
        | Are you able to answer this second question?               | Answer One | /category-one/category-one-section-one/self-assessment/testing-question-two  |

  @clear-data-school
  Scenario: Check answers page change answer link navigates to the correct question
    Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1" and I do not click submit recommendations
    And I click the change link on check answers for "Are you able to choose an answer to this first question?" and should see the question heading
    

