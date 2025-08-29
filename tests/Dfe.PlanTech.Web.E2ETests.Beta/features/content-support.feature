Feature: Content & Support Page Richtext components

  Background:
    Given I visit the information asset register content support page

  Scenario: The page displays an attachment
    Then I should see an attachment block
    And the attachment should include a downloadable link

  Scenario: The page displays accordions
    Then I should see an accordion component
    And I should see a control labelled "Show all sections"
    And I should see multiple accordion body sections
    And I should see multiple accordion toggle labels

  Scenario: Accordion sections are collapsed by default
    Then all accordion sections should be collapsed

  Scenario: Accordion section expands and collapses
    When I expand the first accordion section
    Then the first accordion section should be expanded
    When I collapse the first accordion section
    Then the first accordion section should be collapsed

  Scenario: The page has the correct footer links
    And I should see the GOV.UK footer with expected links
    
  Scenario: The page has the beta header
    And I should see the beta header
