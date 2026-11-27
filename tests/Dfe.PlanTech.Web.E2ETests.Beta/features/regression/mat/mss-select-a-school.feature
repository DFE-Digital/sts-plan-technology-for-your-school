@serial
@user-mat

Feature: MAT - MSS - Select a school page

    @clear-data-school-miscellaneous
    @clear-data-school-foundation
    @clear-data-school-community
    @selected-school-miscellaneous
    Scenario: Setup misc with completed assessments
     Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"

    @selected-school-foundation
    Scenario: Setup foundation with completed assessments
     Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"

    @selected-school-community
    Scenario: Setup community with completed assessments
     Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"


    Scenario: MSS - Select a school Page has all the correct elements
    Given I visit the homepage
    Then I visit the track your schools progress page
    Then I click the card "Category Two"
    And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
    And I click the "Update recommendation status" link
    Then I should see the h1 fieldset heading "Which schools do you want to update the status for?"
    And I should see the caption heading "Recommendation Header - C2 - S1 - Q1"
    And the header should contain all the correct content
    And I should see a back link with govuk class to the previous page
    And I should see the GOV.UK footer with expected links
    And I should see the beta header
    And the page should be accessible

Scenario: MAT user can select multiple schools for multi status update
    Given I visit the homepage
    Then I visit the track your schools progress page
    Then I click the card "Category Two"
    And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
    And I click the "Update recommendation status" link
    Then I should see the following schools and their recommendation statuses:
    | school                                             | lastUpdated | status      |
    | DSI TEST Establishment (001) Community School (01) | today       | Complete    |
    | DSI TEST Establishment (001) Foundation School (05)| today       | Complete    |
    | DSI TEST Establishment (001) Miscellaneous (27)    | today       | Complete    |

  When I select the following schools for multi status update:
    | school                                                   |
    | DSI TEST Establishment (001) Community School (01)       |
    | DSI TEST Establishment (001) Miscellaneous (27)          |
  Then the following schools should be selected for multi status update:
    | school                                                   |
    | DSI TEST Establishment (001) Community School (01)       |
    | DSI TEST Establishment (001) Miscellaneous (27)          |

Scenario: MAT user can select and deselect all schools for multi status update
  Given I visit the homepage
    Then I visit the track your schools progress page
    Then I click the card "Category Two"
    And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
    And I click the "Update recommendation status" link
  When I select all schools for multi status update
  Then all schools should be selected for multi status update
  When I deselect all schools for multi status update
  Then all schools should be deselected for multi status update

Scenario: Selecting every school individually checks the select all checkbox
      Given I visit the homepage
    Then I visit the track your schools progress page
    Then I click the card "Category Two"
    And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
    And I click the "Update recommendation status" link
  When I select the following schools for multi status update:
    | school                                              |
    | DSI TEST Establishment (001) Community School (01)  |
    | DSI TEST Establishment (001) Foundation School (05) |
    | DSI TEST Establishment (001) Miscellaneous (27)     |
  Then all schools should be selected for multi status update
  And the select all checkbox should be checked

  Scenario: Deselecting one school updates the select all checkbox
      Given I visit the homepage
    Then I visit the track your schools progress page
    Then I click the card "Category Two"
    And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
    And I click the "Update recommendation status" link
  When I select all schools for multi status update
  And I deselect the school "DSI TEST Establishment (001) Foundation School (05)"
  Then the select all checkbox should not be checked
  And the following schools should be selected for multi status update:
    | school                                             |
    | DSI TEST Establishment (001) Community School (01) |
    | DSI TEST Establishment (001) Miscellaneous (27)    |



Scenario: MAT user cannot continue without selecting a school
    Given I visit the homepage
    Then I visit the track your schools progress page
    Then I click the card "Category Two"
    And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
    And I click the "Update recommendation status" link
    When I click the "Continue" button
    Then I should see the validation error "Select one, more or all schools"