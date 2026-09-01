@serial
@user-mat

Feature: MAT - MSA - Select a school page

    Scenario: MSA - Select a school Page has all the correct elements
    Given I visit the homepage
    Then I visit the select-a-self-assessment page
    And I click the "Category One Section One" link
    Then I should see the page heading "Section One Interstitial Title"
    And I click the "Continue" link
    Then I should see the h1 fieldset heading "Which schools do you want to submit the self-assessment for?"
    And the header should contain all the correct content
    And I should see a back link with text "Back to category one section one" and URL "/category-one/category-one-section-one/self-assessment"
    And I should see the GOV.UK footer with expected links
    And I should see the beta header
    And the page should be accessible

@clear-data-school-miscellaneous
@clear-data-school-foundation
@clear-data-school-community
Scenario: MAT user can select multiple schools for a self-assessment
    Given I visit the select-a-self-assessment page
    And I click the "Category One Section One" link
    And I click the "Continue" link
  Then I should see the select schools self-assessment page
  And I should see the following schools available for self-assessment:
    | school                                                   |
    | DSI TEST Establishment (001) Community School (01)       |
    | DSI TEST Establishment (001) Miscellaneous (27)          |
    | DSI TEST Establishment (001) Foundation School (05)      |
  When I select the following schools:
    | school                                                   |
    | DSI TEST Establishment (001) Community School (01)       |
    | DSI TEST Establishment (001) Miscellaneous (27)          |
  Then the following schools should be selected:
    | school                                                   |
    | DSI TEST Establishment (001) Community School (01)       |
    | DSI TEST Establishment (001) Miscellaneous (27)          |

Scenario: Selecting all schools clears individually selected schools
    Given I visit the select-a-self-assessment page
    And I click the "Category One Section One" link
    And I click the "Continue" link
    Then I should see the select schools self-assessment page
    When I select the following schools:
            | school                                                   |
            | DSI TEST Establishment (001) Community School (01)       |
            | DSI TEST Establishment (001) Miscellaneous (27)          |
    And I select all schools without a submission
    Then all individual school checkboxes should be unselected
    And the all schools checkbox should be selected

@selected-school-miscellaneous
Scenario: Self assessment in progress displays under the school nam,e
        Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 1" and I do not click submit recommendations
        Then I visit the select-a-self-assessment page
        And I click the "Category One Section One" link
        And I click the "Continue" link
        Then I should see the select schools self-assessment page
        Then the school "DSI TEST Establishment (001) Miscellaneous (27)" should show self-assessment started text