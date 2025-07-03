Feature: Landing page

  Background:
    Given I visit the landing page

  Scenario: The page has a main title
    Then I should see the main page heading

  Scenario: The page has multiple feature headings
    Then I should see multiple feature headings

  Scenario: The page shows explanatory text
    Then I should see multiple explanatory text blocks

  Scenario: The page lists multiple items
    Then I should see multiple list items

  Scenario: The page has a start button
    Then I should see a start button

  Scenario: The start button links to the self-assessment page
    Then the start button should link to "/self-assessment"

  Scenario: The page passes accessibility checks
    Then the page should be accessible
