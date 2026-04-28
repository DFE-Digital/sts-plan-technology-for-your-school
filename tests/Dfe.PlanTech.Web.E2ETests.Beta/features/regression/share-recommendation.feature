
@serial
@user-school

Feature: Recommendations Page

    Background:
    @clear-data-school

     Scenario: Homepage
      Given I visit the self-assessment-testing page
      Then I should see a card for "Category One" with a link labelled "Start Self Assessment"
      When I click the card "Category One"
      Then I should see the page heading "Category One"
