@user-school
Feature: Questions
    Background:
        Given I start an assessment on category "Category One Section One"

    Scenario:
        Then I should see the question heading "Are you able to choose an answer to this first question?"
        And I should see a back link to "/category-one-section-one"
        