@smoke
@user-school
Feature: Static pages

Pages which are non functional to the user journey e.g. Cookies, Privacy Policy, Accessibility Statement

Scenario: Accessibility Page
    Given I am on the accessibility page
    Then I should see the page heading "Accessibility statement for Plan technology for your school"
    And I should see a back link with href containing "/home"

    Then I should see multiple paragraphs of explanatory text
    And I should see a non govuk section heading with text "Feedback and contact information"
    And I should see a non govuk section heading with text "Reporting accessibility problems with this website"
    And I should see a non govuk section heading with text "Enforcement procedure"
    And I should see a non govuk section heading with text "Technical information about this website's accessibility"
    And I should see a non govuk section heading with text "Compliance status"
    And I should see a non govuk section heading with text "Preparation of this accessibility statement"

    Then the page should be accessible

    And I should see the GOV.UK footer with expected links
    And I should see the beta header

Scenario: Cookies Page
    Given I am on the cookies page
    Then I should see the page heading "Cookies"
    And I should see a back link with href containing "home"

    Then I should see multiple paragraphs of explanatory text
    And I should see a h2 section heading with text "Essential cookies"
    And I should see a h2 section heading with text "Optional analytics cookies"
    And I should see a h2 section heading with text "Change your cookie settings"

    Then I should see a cookie preferences form with 2 radio options
    When I choose to accept cookies and save settings
    Then I should see a notification banner confirming the action
    And Google Tag Manager should be enabled

    When I choose to reject cookies and save settings
    Then I should see a notification banner confirming the action
    And Google Tag Manager should be disabled
  
    And I should see the GOV.UK footer with expected links
    And I should see the beta header

Scenario: Not found page returns on paths /non-existent-slug
      Given I visit a non-existent page
      Then I should see the page heading "Page not found"
      Then I should see multiple explanatory text blocks
      Then I should see a contact us link
      And I should see the GOV.UK footer with expected links
      And I should see the beta header

Scenario: Landing page
    Given I visit the landing page
    Then I should see the main page heading
    Then I should see multiple feature headings
    Then I should see multiple explanatory text blocks
    Then I should see multiple list items
    Then I should see a start button
    Then the start button should link to "/home"
    Then the page should be accessible
