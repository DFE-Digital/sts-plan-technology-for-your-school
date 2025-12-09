@serial
@user-mat
@selected-school-miscellaneous
Feature: MAT Single Category Landing Page

  Background:
    @clear-data-school
    Scenario: Category landing page displays correct content when no assessment has been started
      Given I am on the self-assessment testing page and click on the category "Category One"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page
      And I should see a paragraph with text "This is the Category One Section One description"
      And I should not see the confirmation panel for "Category One Section One"
      And I should not see any recommendation links
      And I should see the GOV.UK footer with expected links
      And I should see the beta header
      And the header should contain all the correct content

    Scenario: Completing an assessment shows the correct recommendations on the category landing page
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1"
      Then I should be on the URL containing "/category-one"
      Then I should see the page heading "Category One"
      And I should see the confirmation panel for "Category One Section One"
      And I should see a link to the recommendation "Recommendation Header - C1 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C1 - S1 - Q2"

      When I click the first recommendation link on the category landing page
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"

      When I click the back to "category one" link
      Then I should be on the URL containing "/category-one"

      When I click the recommendation link "Recommendation Header - C1 - S1 - Q1" on the category landing page
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"

    Scenario: Category landing page displays the same recommendations after returning to the page
      Given I visit the self-assessment-testing page
      Then I should see a card for "Category One" with a link labelled "View recommendations"
      When I click the card "Category One"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page
      And I should not see the confirmation panel for "Category One Section One"
      And I should see a link to the recommendation "Recommendation Header - C1 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C1 - S1 - Q2"
      And I should see the completed self-assessment message for "Category One Section One"
