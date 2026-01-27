@serial
@user-school
Feature: Recommendations Status

  Background:
  @clear-data-school
    Scenario: Completing an assessment with completing answers sets the status to complete
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "Complete"
      And recommendation "Recommendation Header - C2 - S1 - Q2" should have status "Complete"
      And recommendation "Recommendation Header - C2 - S1 - Q3" should have status "Complete"

  @clear-data-school
    Scenario: Completing an assessment correctly sets the status to not started
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "3, 2, 2"
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "Not started"
      And recommendation "Recommendation Header - C2 - S1 - Q2" should have status "Not started"
      And recommendation "Recommendation Header - C2 - S1 - Q3" should have status "Not started"

  @clear-data-school
    Scenario: Completing an assessment with an in-progress answer sets the status to in progress
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "3, 2, 4"
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "Not started"
      And recommendation "Recommendation Header - C2 - S1 - Q2" should have status "Not started"
      And recommendation "Recommendation Header - C2 - S1 - Q3" should have status "In progress"

  @clear-data-school
    Scenario: Ability to update the recommendation status to each status
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "3, 2, 4"
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "Not started"
      And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page

      # In Progress
      When I change the recommendation status to "In progress" and save it
      Then I should see a confirmation panel saying "Status updated to 'In progress'"
      Then recommendation status is shown as "In progress"
      Then recommendation status in the header matches the selected status in the form

      When I click the non-js back link
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "In progress"
      Then I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page


      # Complete
      When I change the recommendation status to "Complete" and save it
      Then I should see a confirmation panel saying "Status updated to 'Complete'"
      Then recommendation status is shown as "Complete"
      Then recommendation status in the header matches the selected status in the form

      When I click the non-js back link
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "Complete"
      Then I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page

      # Not started
      When I change the recommendation status to "Not started" and save it
      Then I should see a confirmation panel saying "Status updated to 'Not started'"
      Then recommendation status is shown as "Not started"
      Then recommendation status in the header matches the selected status in the form

      When I click the non-js back link
      Then recommendation "Recommendation Header - C2 - S1 - Q1" should have status "Not started"
      Then I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
