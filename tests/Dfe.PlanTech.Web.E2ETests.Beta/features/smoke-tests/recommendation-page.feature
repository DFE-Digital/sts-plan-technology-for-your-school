@smoke
@user-school
Feature: Recommendations page

  @clear-data-school
  Scenario: User can view and navigate recommendations after completing an assessment
    Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1"
    And I click the link to view the recommendations for "Category One Section One"
    When I click the first recommendation link on the category landing page
    Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"
    And I should see the recommendation caption text "Recommendation 1 of 2"
    And I should see a visible Next pagination link with text "Recommendation Header - C1 - S1 - Q2"

    When I click the next recommendation link
    Then I should see the page heading "Recommendation Header - C1 - S1 - Q2"
    And I should see the recommendation caption text "Recommendation 2 of 2"
    And I should see a visible Previous pagination link with text "Recommendation Header - C1 - S1 - Q1"