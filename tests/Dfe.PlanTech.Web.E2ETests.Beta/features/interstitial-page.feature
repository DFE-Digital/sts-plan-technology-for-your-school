Feature: Interstitial Paage
    Background:
        Given I am on the self-assessment page and click on the category "Category One Section One"

    Scenario:
        Then I should see the page heading "Section One Interstitial Title"
        And I should see a back link to "/self-assessment-testing"
        And I should see multiple explanatory text blocks
        And I should see a button with the text "Start self-assessment"
        And the page should be accessible
        