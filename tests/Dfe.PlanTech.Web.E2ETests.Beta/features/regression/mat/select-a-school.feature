@serial
@user-mat

Feature: MAT user selects a school

  Scenario: MAT sees the select a school page with their schools
    Given I visit the homepage
    Then I should see the select a school page
    And I should see the following schools:
      | DSI TEST Establishment (001) Community School (01) |
      | DSI TEST Establishment (001) Miscellaneous (27)    |
      | DSI TEST Establishment (001) Foundation School (05)|

  Scenario: MAT sees progress for each school
    Given I visit the homepage
    Then I should see the following school progress:
      | School name                                            |
      | DSI TEST Establishment (001) Community School (01)     |
      | DSI TEST Establishment (001) Miscellaneous (27)        |
      | DSI TEST Establishment (001) Foundation School (05)    |

  Scenario: MAT selects a school and is taken to that school’s dashboard
    Given I visit the homepage
    Given I am on the select a school page
    When I select the school "DSI TEST Establishment (001) Community School (01)"
    Then I should be on the URL containing "/home"
    And I should see the selected school name "DSI TEST Establishment (001) Community School (01)"
    And the selected school cookie "SelectedSchool" should have URN "900006" and name "DSI TEST Establishment (001) Community School (01)"

