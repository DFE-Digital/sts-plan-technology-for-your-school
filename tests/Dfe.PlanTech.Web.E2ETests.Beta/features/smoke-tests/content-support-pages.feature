@smoke
Feature: Content & Support Page

  Scenario:
    Given I visit the information asset register content support page
    Then I should see an attachment block
    And the attachment should include a downloadable link
    And I should see the GOV.UK footer with expected links
    And I should see the beta header
