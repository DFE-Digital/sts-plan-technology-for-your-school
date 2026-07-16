@smoke
@user-school
Feature: Static pages

Pages which are non functional to the user journey e.g. Cookies, Privacy Policy, Accessibility Statement

Scenario: Accessibility Page
    Given I visit the homepage
    Then I click the link "Accessibility statement"
    Then I should see the page heading "Accessibility statement"

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

Scenario: Not found page returns on paths /non-existent-slug
      Given I visit a non-existent page
      Then I should see the page heading "Page not found"
      Then I should see a contact us link

Scenario: Landing page
    Given I visit the landing page
    Then I should see the main page heading
    Then I should see multiple feature headings
    Then I should see multiple explanatory text blocks
    Then I should see multiple list items
    Then I should see a start button
    Then the start button should link to "/home"
