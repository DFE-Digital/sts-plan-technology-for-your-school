@smoke
@user-school
Feature: Recommendations Page

  Background:
  @clear-data-school
    Scenario: Completing an assessment shows the correct recommendations
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1"
      When I click the first recommendation link on the category landing page
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"

      And I should see the related actions sidebar
      And I should see the related actions links for category "Category One" section "Category One Section One" recommendation "testing-c1-s1-q1-recommendation"

      And I should see the recommendation caption text "Recommendation 1 of 2"
      And I should see a visible Next pagination link with text "Recommendation Header - C1 - S1 - Q2"
      And I should see the paragraph text "You told us your school has a firewall that is configured to meet your school’s needs." inside the recommendation content

      Then I click the next recommendation link
      And I should see the page heading "Recommendation Header - C1 - S1 - Q2"
      And I should see the recommendation caption text "Recommendation 2 of 2"
      And I should see the paragraph text "This is some recommendation text" inside the recommendation content
      And I should see a visible Previous pagination link with text "Recommendation Header - C1 - S1 - Q1"

      Then I click the previous recommendation link
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"
