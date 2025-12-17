@smoke
@user-mat
Feature: MAT Multi Category Landing Page

  Scenario: MAT sees the select a school page with their schools
    Given I visit the homepage
    Then I should see the select a school heading
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
    And the selected school cookie "SelectedSchool" should have URN "00000002" and name "DSI TEST Establishment (001) Community School (01)"

    @selected-school-miscellaneous
    @clear-data-school
    Scenario: MAT completing one topic on a multi-topic category shows the correct content
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
      Then I should see the confirmation panel for "Category Two Section One"
      And I am on the "category-two" page
      Then I should see the page heading "Category Two"
      Then I should see the "completed" section "Category Two Section One" with description "" and link href "/category-two/category-two-section-one/view-answers"
      And I should see the "not started" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/self-assessment"
      And I should see the "not started" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/self-assessment"

      Then I should see an inset text containing "Complete the remaining self-assessments to get recommendations for your school's category two."
      And in the inset text I should see the following links:
      | Text                                                                | Href                                                                      |
      | Go to self-assessment for category two section two                  | /category-two/category-two-section-two/self-assessment                    |
      | Go to self-assessment for category two section three                | /category-two/category-two-section-three/self-assessment                  |

      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q3"

      And I should not see the confirmation panel for "Category Two Section One"
      And I should not see the confirmation panel for "Category Two Section Two"
      And I should not see the confirmation panel for "Category Two Section Three"


