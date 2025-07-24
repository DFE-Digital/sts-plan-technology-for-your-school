Feature: Change answers page
@clear-data-school
    Scenario: Change answers has all the correct elements on the page
        Given I start a test assessment on "Category One Section One" with answers "1, 1"
        Then I click the view or update self-assessment link for "Category One Section One"
        And I should see the page heading "Change your answers"
        And I should see the caption heading "Category One Section One"
        And I should see the back to recommendations button for "Category One Section One"
        
        And I should see the following change answer rows:
        | Question                                                   | Answer     | Href                                            |
        | Are you able to choose an answer to this first question?   | Answer 1   | /category-one-section-one/testing-question-one  |
        | Are you able to answer this second question?               | Answer One | /category-one-section-one/testing-question-two  |
    
@clear-data-school
    Scenario: Clicking change answers link and changing answers shows correct answers and recommendations
        Given I start a test assessment on "Category One Section One" with answers "1, 1"
        Then I click the view or update self-assessment link for "Category One Section One"
        Then I click the change link on change answers for "Are you able to choose an answer to this first question?" and I should see the question heading
        Then I change the answer to "Answer 2" and continue all other questions
        Then I am on the "category-one-section-one/check-answers?isChangeAnswersFlow=True" page 
        
        And I should see the following check answer rows:
        | Question                                                   | Answer     | Href                                            |
        | Are you able to choose an answer to this first question?   | Answer 2   | /category-one-section-one/testing-question-one  |
        | Are you able to answer this second question?               | Answer One | /category-one-section-one/testing-question-two  |
        
        Then I click the submit and view recommendations button
        Then I should see the page heading "Low Maturity Header"
        And I should see the recommendation caption text "Recommendation 1 of 3"
        And I should see the paragraph text "This is a low maturity recommendation intro text" inside the recommendation content

        And I should see a visible Next pagination link with text "Recommendation Header Answer Two"

        When I click the next recommendation link
        Then I should see the page heading "Recommendation Header Answer Two"
        And I should see the recommendation caption text "Recommendation 2 of 3"
        And I should see a visible Previous pagination link with text "Recommendation Header - Q2 - Answer One"
        And I should see the paragraph text "Your senior leadership team (SLT) digital lead is accountable for making sure your school has an effective firewall." inside the recommendation content

        Then I click the next recommendation link
        And I should see the page heading "Recommendation Header - Q2 - Answer One"
        And I should see the recommendation caption text "Recommendation 3 of 3"
        And I should see the paragraph text "This is some recommendation text" inside the recommendation content
        And I should see a visible Previous pagination link with text "Recommendation Header Answer Two"





