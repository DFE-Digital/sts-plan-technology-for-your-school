@user-school
Feature: home page

  Background:
    Given I visit the homepage

  Scenario: Homepage has the correct header and beta-header
    Then the header should contain all the correct content
    And I should see the beta header

  Scenario: Page has a main heading
    Then I should see the page heading "Technology self‑assessment"

  Scenario: Page contains category headings
    Then I should see a subheading with the text "How to meet the digital and technology standards"

  Scenario: Page contains multiple sections
    Then I should see multiple section cards on the page

  Scenario: Each section has a link
    Then each section card should link to its details page

  Scenario: Page passes accessibility checks
    Then the page should be accessible

  Scenario: Page has correct footer links
    And I should see the GOV.UK footer with expected links
