Feature: Cookies Page

  Background:
    Given I am on the cookies page

  Scenario: Cookies page has correct heading and back link
    Then I should see the page heading "Cookies"
    And I should see a back link with href containing "self-assessment"

  Scenario: Cookies page has explanatory text and sections
    Then I should see multiple paragraphs of explanatory text
    And I should see a section heading with text "Essential cookies"
    And I should see a section heading with text "Optional analytics cookies"
    And I should see a section heading with text "Change your cookie settings"

  Scenario: Cookie preferences form is present
    Then I should see a cookie preferences form with 2 radio options

  Scenario: Accept cookies updates the page
    When I choose to accept cookies and save settings
    Then I should see a notification banner confirming the action
    And Google Tag Manager should be enabled

  Scenario: Reject cookies updates the page
    When I choose to reject cookies and save settings
    Then I should see a notification banner confirming the action
    And Google Tag Manager should be disabled
