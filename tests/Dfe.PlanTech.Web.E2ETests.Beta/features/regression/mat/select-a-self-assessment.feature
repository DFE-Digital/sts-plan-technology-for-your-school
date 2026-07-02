@serial
@user-mat

Feature: MAT user selects a self assessment

  Scenario: MAT sees the select a self assessment page with their schools
    Given I visit the homepage
    Given I am on the select a school page
    When I select the school "DSI TEST Establishment (001) Community School (01)"
    Then I should be on the URL containing "/home"
    When I visit the page "groups/select-a-self-assessment"
    Then I should see the select a self assessment page
    Then I should see the following self assessments:
      |          Category One Section One                      |
      |          Category Two Section One                      |
      |          Category Two Section Two                      |
      |          Category Two Section Three                    |

    Scenario: MAT selects a self assessment category
      Given I visit the homepage
      Given I am on the select a school page
      When I select the school "DSI TEST Establishment (001) Community School (01)"
      Then I should be on the URL containing "/home"
      When I visit the page "groups/select-a-self-assessment"
      Then I should see the select a self assessment page
      And I select the self assessment "Category One Section One"
      Then I should see the page heading "Section One Interstitial Title"