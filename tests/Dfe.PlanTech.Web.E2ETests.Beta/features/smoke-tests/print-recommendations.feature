@smoke
@user-school
Feature: Print recommendations page

  @clear-data-school
  Scenario: User can view the print recommendations page
    Given I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
    And I click the link to view the recommendations for "Category Two Section Three"
    And I click the recommendation link "Recommendation Header - C2 - S3 - Q5" on the category landing page
    When I click the print all recommendations link in the related actions for "Category Two Section Three"
    Then I should see the page heading "Category Two"
    And I should see a h2 section heading with text "Category Two Section Three"
    And I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q5"
    And I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q1"
    And I should see the print this page button