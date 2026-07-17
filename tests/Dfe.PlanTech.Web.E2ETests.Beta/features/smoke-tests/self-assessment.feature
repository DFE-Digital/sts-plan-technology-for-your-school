@smoke
@user-school
Feature: Multi Category Landing Page

  @clear-data-school
  Scenario: Multi category landing page shows sections before assessments are started
    Given I am on the self-assessment testing page and click on the category "Category Two"
    Then I should see the page heading "Category Two"
    And I am on the "category-two" page
    And I should see the "not started" section "Category Two Section One" with description "This is the Category Two Section One Description" and link href "/category-two/category-two-section-one/self-assessment"
    And I should see the "not started" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/self-assessment"
    And I should see the "not started" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/self-assessment"
    And I should not see any recommendation links

  @clear-data-school
  Scenario: Multi category landing page shows completed sections and recommendations after assessments are completed
    Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
    And I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "3, 2, 2"
    And I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
    And I click the link to view the recommendations for "Category Two Section Three"
    And I am on the "category-two" page
    Then I should see the page heading "Category Two"
    And I should see the "completed" section "Category Two Section One" with description "This is the Category Two Section One Description" and link href "/category-two/category-two-section-one/view-answers"
    And I should see the "completed" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/view-answers"
    And I should see the "completed" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/view-answers"
    And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q1"
    And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q1"
    And I should see a link to the recommendation "Recommendation Header - C2 - S3 - Q5"