@serial
@user-school
Feature: Recommendation recent activity

  Background:
    @clear-data-school

    Scenario: Completing an assessment shows the correct initial recent activity 
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1"
      Then I should be on the URL containing "/category-one"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page
      
      When I click the recommendation link "Recommendation Header - C1 - S1 - Q1" on the category landing page
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"

      When I click "Hide all sections" on the recent activity accordion
      Then all recent activity accordion sections should be collapsed
      When I click "Show all sections" on the recent activity accordion
      Then all recent activity accordion sections should be expanded

      When I expand the "February activity" section in recent activity
      Then the "February activity" section should contain a recent activity entry with status "Complete" and question "Are you able to choose an answer to this first question?" dated today

    @clear-data-school
    Scenario: Changing status on a recommendation updates the recent activity.
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1"
      Then I should be on the URL containing "/category-one"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page
      
      When I click the recommendation link "Recommendation Header - C1 - S1 - Q1" on the category landing page
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"

      Then I set the recommendation status to "In progress" with note "I have updated the status." and submit
      Then the section should show a status change to "In progress" with note "I have updated the status." dated today

      Then I set the recommendation status to "Not started" with note "I have updated the status again." and submit
      Then the section should show a status change to "Not started" with note "I have updated the status again." dated today

      Then I set the recommendation status to "Complete" with note "I have updated the status to complete." and submit
      Then the section should show a status change to "Complete" with note "I have updated the status to complete." dated today




