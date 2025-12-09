@smoke
@user-no-org
Feature: Unauthorised access

    Scenario: Unauthorised page has the correct header
        Given I visit the homepage
        Then I should be on the URL containing "/dsi-error-not-associated-organisation"
        Then the header should contain all the correct content
        Then I should see the page heading "You are not associated with an organisation"
        And I should see a link to log into DfE Sign-in
        And I should see the GOV.UK footer with expected links
        And I should see the beta header

