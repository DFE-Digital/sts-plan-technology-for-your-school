@user-school
Feature: 404 Not Found page

  Background:
    Given I visit a non-existent page

  Scenario: The 404 not found page has the correct elements
    Then I should see the page heading "Page not found"

  Scenario: The page shows explanatory text
    Then I should see multiple explanatory text blocks

  Scenario: The page includes a contact us link
    Then I should see a contact us link

  Scenario: The page has the correct footer links
    And I should see the GOV.UK footer with expected links

  Scenario: The page has the beta header
    And I should see the beta header
