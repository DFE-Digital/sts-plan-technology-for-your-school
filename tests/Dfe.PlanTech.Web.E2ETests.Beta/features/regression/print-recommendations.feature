@serial
@user-school
Feature: Print recommendations Page

  Background:
  @clear-data-school
    Scenario: Print recommendations page has correct layout content
        Given I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
        And I click the recommendation link "Recommendation Header - C2 - S3 - Q5" on the category landing page
        And I click the print all recommendations link in the related actions for "Category Two Section Three"

        Then I should see the GOV.UK footer with expected links
        And I should see the beta header
        And the header should contain all the correct content
        And I should see the page heading "Category Two"
        And I should see a h2 section heading with text "Category Two Section Three"
        And I should see the print this page button

        When I click the print this page button
        Then the print dialog should be triggered

        Then I click the js-back link
        And I am on the "category-two/category-two-section-three/recommendations/testing-c2-s3-q5-recommendation" page

    Scenario: Print recommendations page has correct recommendations content
        Given I visit the self-assessment-testing page
        And I click the card "Category Two"
        And I click the recommendation link "Recommendation Header - C2 - S3 - Q5" on the category landing page
        And I click the print all recommendations link in the related actions for "Category Two Section Three"
        And I should see a js-back link to "category-two/category-two-section-three/recommendations/testing-c2-s3-q5-recommendation"

        Then I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q5"
        And I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q4"
        And I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q3"
        And I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q2"
        And I should see the print page recommendation text "Recommendation Header - C2 - S3 - Q1"

        Then I should see a recommendation with heading "Recommendation Header - C2 - S3 - Q5" and content containing "This is the content for Category Two Section Three Question Five Answer Two" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header - C2 - S3 - Q4" and content containing "This is the content for Category Two Section Three Question Four Answer One" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header - C2 - S3 - Q3" and content containing "This is the content for Category Two Section Three Question Three Answer Two" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header - C2 - S3 - Q2" and content containing "This is the content for Category Two Section Three Question Two Answer One" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header - C2 - S3 - Q1" and content containing "This is the content for Category Two Section Three Question One Answer One" on the print recommendation page
