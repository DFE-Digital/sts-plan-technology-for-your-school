Feature: Beta Phase Banner

  Background:
    Given I visit the homepage

  Scenario: Phase banner is visible
    Then I should see the phase banner

  Scenario: Phase banner displays Beta tag
    Then I should see the phase banner tag with text "Beta"

  Scenario: Phase banner contains feedback link
    Then I should see a feedback link in the phase banner with href containing "https://forms.office.com/e/Jk5PuNWvGe"
