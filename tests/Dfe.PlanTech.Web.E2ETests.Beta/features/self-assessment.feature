@user-school
Feature: Self-assessment page

  Background:
    Given I am logged in and visit the self-assessment page

  Scenario: Page has a main heading
    Then I should see the page heading "Technology self‑assessment"

  Scenario: Page contains category headings
    Then I should see a subheading with the text "Digital leadership and governance"
    Then I should see a subheading with the text "Connectivity"
    Then I should see a subheading with the text "Filtering and monitoring"

  Scenario: Page contains multiple sections
    Then I should see multiple section cards on the page

  Scenario: Each section has a link
    Then each section card should link to its details page

  Scenario: Page passes accessibility checks
    Then the page should be accessible
