@serial
@user-school
Feature: Recommendation recent activity

  Background:
    @clear-data-school
    Scenario: Sorting recommendations as default on category landing page updates table correctly.
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 2"
      Then I should be on the URL containing "/category-one"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page
      Then I sort recommendations by "Default" and apply
      Then the recommendations table should list recommendations in this order:
        | Recommendation Header - C1 - S1 - Q1 |
        | Recommendation Header - C1 - S1 - Q2 |
    
    @clear-data-school
    Scenario: Sorting recommendations as Status on category landing page updates table correctly.
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 2"
      Then I should be on the URL containing "/category-one"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page
      Then I sort recommendations by "Status" and apply
      Then the recommendations table should list recommendations in this order:
        | Recommendation Header - C1 - S1 - Q2 |
        | Recommendation Header - C1 - S1 - Q1 |

    @clear-data-school
    Scenario: Sorting recommendations as Last updated on category landing page updates table correctly.
      Given I start a test assessment on "Category One" category "Category One Section One" section with answers "1, 2"
      Then I should be on the URL containing "/category-one"
      Then I should see the page heading "Category One"
      And I am on the "category-one" page

      When I click the recommendation link "Recommendation Header - C1 - S1 - Q1" on the category landing page
      Then I should see the page heading "Recommendation Header - C1 - S1 - Q1"

      Then I set the recommendation status to "In progress" with note "I have updated the status." and submit
      When I click the non-js back link

      Then I sort recommendations by "Last updated" and apply
      Then the recommendations table should list recommendations in this order:
        | Recommendation Header - C1 - S1 - Q1 |
        | Recommendation Header - C1 - S1 - Q2 |  


      