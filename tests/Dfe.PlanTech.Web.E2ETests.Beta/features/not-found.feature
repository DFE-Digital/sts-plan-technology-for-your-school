@user-school
Feature: 404 Not Found page

  Background:
    Given I visit a non-existent page

  Scenario: The page shows a main heading
    Then I should see the page heading "Page not found"

  Scenario: The page shows explanatory text
  Then I should see multiple explanatory text blocks

  Scenario: The page includes a contact us link
    Then I should see a contact us link

