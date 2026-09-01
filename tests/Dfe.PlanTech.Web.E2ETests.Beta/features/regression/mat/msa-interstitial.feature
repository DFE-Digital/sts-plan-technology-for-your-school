@serial
@user-mat

Feature: MAT - MSA Interstitial Page
    @clear-data-school-miscellaneous
    @clear-data-school-foundation
    @clear-data-school-community
    Scenario: MSA - Interstitial Page has all the correct elements
    Given I visit the homepage
    Then I visit the select-a-self-assessment page
    And I click the "Category One Section One" link
    Then I should see the page heading "Section One Interstitial Title"
    And the header should contain all the correct content
    And I should see a back link with text "Back to select a self-assessment" and URL "/groups/select-a-self-assessment"
    And I should see the GOV.UK footer with expected links
    And I should see the beta header
    And the page should be accessible

    @selected-school-miscellaneous
    Scenario: MAT - MSA Interstitial Page - Schools component
        Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1" and I do not click submit recommendations
        Then I visit the select-a-self-assessment page
        And I click the "Category One Section One" link
        Then I should see the page heading "Section One Interstitial Title"
        Then I should see the MAT self-assessment school selection content
        And I should see the following school self-assessment statuses:
            | school                                                   | status              |
            | DSI TEST Establishment (001) Community School (01)      | Not started         |
            | DSI TEST Establishment (001) Miscellaneous (27)         | Answers in progress |
            | DSI TEST Establishment (001) Foundation School (05)     | Not started         |
        And I should see the continue button for selecting schools

