@serial
@user-mat

Feature: MAT - Multi Status Update for Schools

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

Scenario: MAT user can update recommendation status for multiple schools
  Given I visit the homepage
  Then I visit the track your schools progress page
  Then I click the card "Category Two"
  And I click the recommendation link "Recommendation Header - C2 - S1 - Q1" on the category landing page
  And I click the "Update recommendation status" link

  When I select the following schools for multi status update:
    | school                                              |
    | DSI TEST Establishment (001) Community School (01)  |
    | DSI TEST Establishment (001) Foundation School (05) |

  And I click the "Continue" button

  Then I should see the multi status update page
  And I should see the following schools selected for status update:
    | school                                              |
    | DSI TEST Establishment (001) Community School (01)  |
    | DSI TEST Establishment (001) Foundation School (05) |

  When I select the recommendation status "In progress"
  And I enter the recommendation status comment "Updated by MAT E2E test"
  And I click the "Save" button
  Then I should see the multi status update confirmation for 2 schools
  And I should see the following school recommendation statuses:
    | school                                              | lastUpdated | status      |
    | DSI TEST Establishment (001) Community School (01)  | today       | In progress |
    | DSI TEST Establishment (001) Foundation School (05) | today       | In progress |
    | DSI TEST Establishment (001) Miscellaneous (27)     | today       | Complete    |