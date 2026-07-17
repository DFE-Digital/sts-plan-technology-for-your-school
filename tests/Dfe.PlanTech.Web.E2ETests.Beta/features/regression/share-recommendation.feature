@serial
@user-school

Feature: Share Recommendations
    Background:
    @clear-data-school
     Scenario: Share recommendation by email
     Given I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
        And I click the link to view the recommendations for "Category Two Section Three"
        And I click the recommendation link "Recommendation Header - C2 - S3 - Q5" on the category landing page
        And I click the share recommendations link

        Then I should see the GOV.UK footer with expected links
        And I should see the beta header
        And the header should contain all the correct content
        And I should see the page heading "Share a recommendation by email"
        And I set the email to "technology.planning@education.gov.uk"
        And I set the name to "Test Name"
        And I set the note to "This is a test email"
        When I click the send email button
        Then I see the email has been sent header with the email "technology.planning@education.gov.uk"

    Scenario: Fail to share recommendation by email
     Given I am on the self-assessment testing page and click on the category "Category Two"
        And I click the recommendation link "Recommendation Header - C2 - S3 - Q5" on the category landing page
        And I click the share recommendations link

        Then I should see the GOV.UK footer with expected links
        And I should see the beta header
        And the header should contain all the correct content
        And I should see the page heading "Share a recommendation by email"
        And I set the email to "failed-email@education.gov.uk"
        And I set the name to "Test Name"
        And I set the note to "This is a test email"
        When I click the send email button
        Then I should see the page heading "Sorry, there was a problem"
