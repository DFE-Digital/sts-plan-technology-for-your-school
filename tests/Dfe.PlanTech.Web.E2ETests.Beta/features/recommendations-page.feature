@user-school
Feature: Recommendations Page

  Background:

  Scenario: Completing assessment shows recommendations
    Given I start a test assessment on "Category One Section One" with answers "1, 1"
    Then I should see the page heading "High Maturity Header"
    And I should see the recommendation caption text "Recommendation 1 of 3"
    And I should see the GOV.UK footer with expected links
    And I should see the paragraph text "This is a high maturity recommendation intro text" inside the recommendation content

    Then I should see the related actions sidebar
    And the sidebar link with text "View or update your self-assessment for category one section one" should have href "/category-one-section-one/change-answers"
    And the sidebar link with text "Print all recommendations for category one section one" should have href "category-one-section-one-intro-high/print"
    
    And I should see a visible Next pagination link with text "Recommendation Header - Q1 - Answer One"

    When I click the next recommendation link
    Then I should see the page heading "Recommendation Header - Q1 - Answer One"
    And I should see the recommendation caption text "Recommendation 2 of 3"
     And I should see a visible Previous pagination link with text "Overview"
     And I should see a visible Previous pagination link with text "Recommendation Header - Q2 - Answer One"
     And I should see the paragraph text "You told us your school has a firewall that is configured to meet your school’s needs." inside the recommendation content

     Then I click the next recommendation link
     And I should see the page heading "Recommendation Header - Q2 - Answer One"
    And I should see the recommendation caption text "Recommendation 3 of 3"
     And I should see the paragraph text "This is some recommendation text" inside the recommendation content
     And I should see a visible Previous pagination link with text "Recommendation Header - Q1 - Answer One"
  
Scenario: Homepage shows View recommendations after completing an assessment
    Given I visit the self-assessment-testing page
    Then I should see a card for "Category One Section One" with a link labelled "View recommendations"
    When I click the card "Category One Section One"
    Then I should see the page heading "High Maturity Header"
    And I should see the recommendation caption text "Recommendation 1 of 3"
    And I should see the related actions sidebar