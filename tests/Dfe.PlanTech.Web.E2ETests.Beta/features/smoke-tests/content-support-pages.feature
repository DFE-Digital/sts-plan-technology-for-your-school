@smoke
Feature: Content & Support Page 

  Scenario: 
    Given I visit the information asset register content support page
    Then I should see an attachment block
    And the attachment should include a downloadable link

    Then I should see an accordion component
    And I should see a control labelled "Show all sections"
    And I should see multiple accordion body sections
    And I should see multiple accordion toggle labels

    Then all accordion sections should be collapsed

    When I expand the first accordion section
    Then the first accordion section should be expanded
    When I collapse the first accordion section
    Then the first accordion section should be collapsed

    And I should see the GOV.UK footer with expected links
    
    And I should see the beta header
