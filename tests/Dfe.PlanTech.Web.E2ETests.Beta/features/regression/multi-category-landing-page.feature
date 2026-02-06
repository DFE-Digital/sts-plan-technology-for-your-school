@serial
@user-school
Feature: Multi Category Landing Page

  Background:
    @clear-data-school
    Scenario: Multi category landing page displays correct content when no assessments have been started
      Given I am on the self-assessment testing page and click on the category "Category Two"
      Then I should see the page heading "Category Two"
      And I am on the "category-two" page
      And I should see the GOV.UK footer with expected links
      And I should see the beta header
      And the header should contain all the correct content
      And I should see the "not started" section "Category Two Section One" with description "This is the Category Two Section One Description" and link href "/category-two/category-two-section-one/self-assessment"
      And I should see the "not started" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/self-assessment"
      And I should see the "not started" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/self-assessment"
      And I should see an inset text containing "You have not completed any self-assessments for the category two standard. Complete self-assessments to view all recommendations."
      And I should not see the confirmation panel for "Category Two Section One"
      And I should not see the confirmation panel for "Category Two Section Two"
      And I should not see the confirmation panel for "Category Two Section Three"
      And I should not see any recommendation links

    @clear-data-school
    Scenario: Completing one topic on a multi-topic category shows the correct content
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

    @clear-data-school
    Scenario: Completing one topic and starting another on a multi-topic category shows the correct content
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
      And I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "1, 1" and I do not click submit recommendations
      And I am on the "category-two" page
      Then I should see the page heading "Category Two"
      Then I should see the "completed" section "Category Two Section One" with description "" and link href "/category-two/category-two-section-one/view-answers"
      And I should see the "in progress" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/self-assessment/continue"
      And I should see the "not started" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/self-assessment"

      Then I should see an inset text containing "Complete the remaining self-assessments to get recommendations for your school's category two."
      And in the inset text I should see the following links:
      | Text                                                                | Href                                                                      |
      | Continue your self-assessment for category two section two          | /category-two/category-two-section-two/self-assessment/continue     |
      | Go to self-assessment for category two section three                | /category-two/category-two-section-three/self-assessment                  |

      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q3"

      And I should not see the confirmation panel for "Category One Section One"
      And I should not see the confirmation panel for "Category One Section Two"
      And I should not see the confirmation panel for "Category One Section Three"

    @clear-data-school
    Scenario: Completing two topics on a multi-topic category shows the correct content
      Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
      And I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "3, 2, 2"
      And I am on the "category-two" page
      Then I should see the page heading "Category Two"
      Then I should see the "completed" section "Category Two Section One" with description "" and link href "/category-two/category-two-section-one/view-answers"
      And I should see the "completed" section "Category Two Section Two" with description "This is the Category Two Section Two description" and link href "/category-two/category-two-section-two/view-answers"
      And I should see the "not started" section "Category Two Section Three" with description "This is the Category Two Section Three description" and link href "/category-two/category-two-section-three/self-assessment"

      Then I should see an inset text containing "Complete the remaining self-assessments to get recommendations for your school's category two."
      And in the inset text I should see the following links:
      | Text                                                                | Href                                                                      |
      | Go to self-assessment for category two section three                | /category-two/category-two-section-three/self-assessment                  |

      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q3"

      And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q2"
      And I should see a link to the recommendation "Recommendation Header - C2 - S2 - Q3"

      And I should not see the confirmation panel for "Category Two Section One"
      And I should not see the confirmation panel for "Category Two Section Two"
      And I should not see the confirmation panel for "Category Two Section Three"

    Scenario: Category landing page displays the same recommendations after returning to the page
      Given I visit the self-assessment-testing page
      Then I should see a card for "Category Two" with a link labelled "Continue self-assessments"
      When I click the card "Category Two"
      Then I should see the page heading "Category Two"
      And I am on the "category-two" page
      And I should not see the confirmation panel for "Category One Section One"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q1"
      And I should see a link to the recommendation "Recommendation Header - C2 - S1 - Q2"
      And I should see the "completed" section "Category Two Section One" with description "This is the Category Two Section One Description" and link href "/category-two/category-two-section-one/view-answers"

    @clear-data-school
    Scenario: Completing all topics on a multi-topic category shows the correct content
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
