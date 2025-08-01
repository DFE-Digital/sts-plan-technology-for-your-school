Feature: Accessibility Page

  Background:
    Given I am on the accessibility page

  Scenario: Accessibility page has correct heading and back link
    Then I should see the page heading "Accessibility statement for Plan technology for your school"
    And I should see a back link with href containing "/home"

  Scenario: Accessibility page contains key content sections
    Then I should see multiple paragraphs of explanatory text
    And I should see a non govuk section heading with text "Feedback and contact information"
    And I should see a non govuk section heading with text "Reporting accessibility problems with this website"
    And I should see a non govuk section heading with text "Enforcement procedure"
    And I should see a non govuk section heading with text "Technical information about this website's accessibility"
    And I should see a non govuk section heading with text "Compliance status"
    And I should see a non govuk section heading with text "Preparation of this accessibility statement"

  Scenario: Accessibility page includes contact links
    Then I should see a link with text "contact us page (opens in new tab)"
    And I should see a link with text "contact us (opens in new tab)"
    And I should see a link with text "Equality Advisory and Support Service (EASS) (opens in new tab)"

  Scenario: Accessibility page passes accessibility checks
    Then the page should be accessible
