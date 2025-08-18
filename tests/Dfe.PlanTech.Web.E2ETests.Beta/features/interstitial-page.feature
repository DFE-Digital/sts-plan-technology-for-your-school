@clear-data-school
Feature: Interstitial Paage
    Background:
        Given I am on the self-assessment testing page and click on the category "Category One"

    Scenario:
        Then I am on the "category-one" page
        Then I click the go to self-assessment link for "Category One Section One"
        Then I should see the page heading "Section One Interstitial Title"
        And I should see a back link to "/category-one"
        And I should see multiple explanatory text blocks
        And I should see a button with the text "Start self-assessment"
        And the page should be accessible
        