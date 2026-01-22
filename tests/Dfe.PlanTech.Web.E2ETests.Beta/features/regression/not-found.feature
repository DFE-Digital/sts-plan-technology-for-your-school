@user-school
Feature: Not found page

  Background:
    Scenario: Not found page returns on paths /non-existent-slug
      Given I visit a non-existent page
      Then I should see the page heading "Page not found"
      Then I should see multiple explanatory text blocks
      Then I should see a contact us link
      And I should see the GOV.UK footer with expected links
      And I should see the beta header

    Scenario: Not found page returns on paths {category-here}/{section-here}/non-existent-page
      Given I visit the page "category-one-section-one/category-one-section-one/non-existent-page"
      Then I should see the page heading "Page not found"

    Scenario:Not found page returns on paths {category-here}/non-existent-page
      Given I visit the page "category-one-section-one/non-existent-page"
      Then I should see the page heading "Page not found"
