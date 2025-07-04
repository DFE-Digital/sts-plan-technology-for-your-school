@user-school
Feature: Footer on different pages

  Scenario Outline: Footer is visible on key pages
    Given I am on the "self-assessment" page
    Then I should see the footer
    And I should see a footer link with text "Cookies" and href containing "/cookies"
    And I should see a footer link with text "Privacy notice" and href containing "/privacy-notice"
    And I should see a footer link with text "Contact us" and href containing "https://schooltech.support.education.gov.uk/hc/en-gb/requests/new?ticket_form_id=22398112507922"
    And I should see a footer link with text "Accessibility statement" and href containing "/accessibility-statement"

  Scenario Outline: Footer is visible on key pages
    Given I am on the "information-asset-register" page
    Then I should see the footer
    And I should see a footer link with text "Cookies" and href containing "/cookies"
    And I should see a footer link with text "Privacy notice" and href containing "/privacy-notice"
    And I should see a footer link with text "Contact us" and href containing "https://schooltech.support.education.gov.uk/hc/en-gb/requests/new?ticket_form_id=22398112507922"
    And I should see a footer link with text "Accessibility statement" and href containing "/accessibility-statement"