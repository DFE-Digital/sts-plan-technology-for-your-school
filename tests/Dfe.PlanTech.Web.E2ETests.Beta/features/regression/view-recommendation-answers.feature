@serial
@user-school
Feature: View recommendations answer

  Background:
  @clear-data-school
    Scenario: View recommendation answers have correct layout content
        Given I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
        And I click the view all recommendations answers link for "category two section three"

        Then I should see the GOV.UK footer with expected links
        And I should see the beta header
        And the header should contain all the correct content
        And I should see the page heading "View your answers"
        And I should see the print this page button

        When I click the print this page button
        Then the print dialog should be triggered

    Scenario: View recommendation answers have correct content
    Given I visit the self-assessment-testing page
        And I click the card "Category Two"
        And I click the view all recommendations answers link for "category two section three"

        Then I should see the question "Do you own a car?" with the answer "Yes"
        And I should see the question "Which of these is a fruit?" with the answer "Apple"
        And I should see the question "What colour can an apple be?" with the answer "Green"
        And I should see the question "Which of these are known as the \"Red planet\"?" with the answer "Mars"
        And I should see the question "What do bees make?" with the answer "Honey"
