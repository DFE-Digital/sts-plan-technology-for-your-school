@serial
@user-mat

Feature: MAT - MSA Select a Self-Assessment

    Scenario: Select a self-assessment page has all the correct elements
    Given I visit the select-a-self-assessment page
    Then I should see the page heading "Select a self-assessment"
    And the header should contain all the correct content
    And I should see a back link with text "Back to home" and URL "select-a-school"
    And I should see the GOV.UK footer with expected links
    And I should see the beta header
    And the page should be accessible
    And I should see the following self-assessments in the "Category One" section:
    | self-assessment             |
    | Category One Section One    |

    And I should see the following self-assessments in the "Category Two" section:
    | self-assessment             |
    | Category Two Section One    |
    | Category Two Section Two    |
    | Category Two Section Three  |

    @clear-data-school-miscellaneous
    @clear-data-school-foundation
    @clear-data-school-community
    @selected-school-miscellaneous
    Scenario: Setup misc with completed assessments
     Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
     Given I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "3, 2, 2"

    @selected-school-foundation
    Scenario: Setup foundation with completed assessments
     Given I start a test assessment on "Category Two" category "Category Two Section One" section with answers "4, 1, 1"
     Given I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "3, 2, 2"

    @selected-school-community
    Scenario: Setup community with completed assessments
     Given I start a test assessment on "Category Two" category "Category Two Section Two" section with answers "3, 2, 2"


    Scenario: MAT user sees the correct self-assessment statuses for Category Two
    Given I visit the select-a-self-assessment page
     Then I should see the "Category Two" self-assessment section
     And I should see the following self-assessments in the "Category Two" section:
        | self-assessment             |
        | Category Two Section One   |
        | Category Two Section Two   |
        | Category Two Section Three |
     And the "Category Two Section One" self-assessment status should be "Self-assessment required for 1 school"
     And the "Category Two Section Two" self-assessment status should be "All self-assessments submitted."
     And the "Category Two Section Three" self-assessment status should be "Self-assessment required for 3 schools"
     And I should see the following links in the "Category One" self-assessment section:
        | text                       | url                                                               |
        | Category One Section One   | /category-one/category-one-section-one/self-assessment            |
    And I should see the following links in the "Category Two" self-assessment section:
        | text                       | url                                                               |
        | Category Two Section One   | /category-two/category-two-section-one/self-assessment            |
        | Category Two Section Three | /category-two/category-two-section-three/self-assessment          |

    And "Category Two Section Two" should not be a link in the "Category Two" self-assessment section
