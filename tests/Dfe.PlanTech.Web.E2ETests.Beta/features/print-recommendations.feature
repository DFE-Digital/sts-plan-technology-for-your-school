@user-school
Feature: Print recommendations Page

  Background:
  @clear-data-school
    Scenario: Print recommendations page has correct layout content
        Given I start a test assessment on "Category Two" category "Category Two Section Three" section with answers "1, 1, 1, 2, 4"
        And I click the recommendation link "Recommendation Header C2 - S3 - Q5 - Answer Two" on the category landing page
        And I click the print all recommendations link in the related actions

        Then I should see the GOV.UK footer with expected links
        And I should see the beta header
        And the header should contain all the correct content
        And I should see the print recommendations caption text "How to meet the standard"
        And I should see the page heading "Category Two"
        And I should see a h2 section heading with text "Category Two Section Three"
        And I should see the print this page button
        
        When I click the print this page button
        Then the print dialog should be triggered
      
        Then I click the back link
        And I am on the "category-two/category-two-section-three/recommendations/recommendation-header-c2---s3---q5---answer-two" page

    Scenario: Print recommendations page has correct recommendations content
        Given I visit the self-assessment-testing page
        And I click the card "Category Two"
        And I click the recommendation link "Recommendation Header C2 - S3 - Q5 - Answer Two" on the category landing page
        And I click the print all recommendations link in the related actions
        And I should see a back link to "/category-two/category-two-section-three/recommendations/recommendation-header-c2---s3---q5---answer-two"

        Then I should see the print page recommendation text "Recommendation Header C2 - S3 - Q5 - Answer Two"
        And I should see the print page recommendation text "Recommendation Header C2 - S3 - Q4 - Answer One"
        And I should see the print page recommendation text "Recommendation Header C2 - S3 - Q3 - Answer Two"
        And I should see the print page recommendation text "Recommendation Header C2 - S3 - Q2 - Answer One"
        And I should see the print page recommendation text "Recommendation Header C2 - S3 - Q1 - Answer One"

        Then I should see a recommendation with heading "Recommendation Header C2 - S3 - Q5 - Answer Two" and content containing "This is the content for Category Two Section Three Question Five Answer Two" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header C2 - S3 - Q4 - Answer One" and content containing "This is the content for Category Two Section Three Question Four Answer One" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header C2 - S3 - Q3 - Answer Two" and content containing "This is the content for Category Two Section Three Question Three Answer Two" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header C2 - S3 - Q2 - Answer One" and content containing "This is the content for Category Two Section Three Question Two Answer One" on the print recommendation page
        Then I should see a recommendation with heading "Recommendation Header C2 - S3 - Q1 - Answer One" and content containing "This is the content for Category Two Section Three Question One Answer One" on the print recommendation page

        Then the print recommendation page self assessment summary should show:
        | question                                            | answer |
        | Do you own a car?                                   | Yes    |
        | Which of these is a fruit?                          | Apple  |
        | What colour can an apple be?                        | Green  |
        | Which of these are known as the "Red planet"?       | Mars   |
        | What do bees make?                                  | Honey  |




