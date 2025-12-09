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
    Scenario: MAT completing all topics on a multi-topic category shows the correct content
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
      And I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "3, 2, 2"
      And I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
      And I am on the "category-two" page
      Then I should see the page heading "Category Two"
      Then I should see the "completed" section "Category Two Section One" with description "This is the Category Two Section One Description" and link href "/category-two/category-two-section-one/view-answers"
      And I should see the "completed" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/view-answers"
      And I should see the "completed" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/view-answers"

      # Section One
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q3"

      # Section Two
      And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q3"

      # Section Three
      And I should see a link to the recommendation "Recommendation Header - C2 - S3 - Q5"
      And I should see a link to the recommendation "Recommendation Header - C2 - S3 - Q4"
      And I should see a link to the recommendation "Recommendation Header - C2 - S3 - Q3"
      And I should see a link to the recommendation "Recommendation Header - C2 - S3 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S3 - Q1"

      And I should not see the confirmation panel for "Category Two Section One"
      And I should not see the confirmation panel for "Category Two Section Two"
      And I should not see the confirmation panel for "Category Two Section Three"


