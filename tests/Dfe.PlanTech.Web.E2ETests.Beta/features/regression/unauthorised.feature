@user-no-org
Feature: Unauthorised
    Background:
        Given I visit the homepage
        Then I should be on the URL containing "/dsi-error-not-associated-organisation"

    Scenario: Unauthorised page has the correct header
        Then the header should contain all the correct content

    Scenario: Page has a main heading
        Then I should see the page heading "You are not associated with an organisation"

    Scenario: Login link is present on the page
        And I should see a link to log into DfE Sign-in

    Scenario: The page has the correct footer links
        And I should see the GOV.UK footer with expected links

    Scenario: The page has the beta header
        And I should see the beta header
