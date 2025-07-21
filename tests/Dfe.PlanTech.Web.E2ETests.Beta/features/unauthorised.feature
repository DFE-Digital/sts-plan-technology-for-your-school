@user-no-org
Feature: Unauthorised
    Scenario:
        Given I am logged in and visit the self-assessment page
        Then I will be redirected to "/dsi-error-not-associated-organisation"

