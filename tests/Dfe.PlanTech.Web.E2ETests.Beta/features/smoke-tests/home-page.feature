@smoke
@user-school
Feature: home page
    
  Scenario: Homepage has the correct header and beta-header
    Given I visit the homepage
    Then the header should contain all the correct content
    And I should see the beta header

    Then I should see the page heading "Technology self‑assessment"

    Then I should see a subheading with the text "How to meet the digital and technology standards"

    Then I should see multiple section cards on the page

    Then each section card should link to its details page

    Then the page should be accessible

    And I should see the GOV.UK footer with expected links
