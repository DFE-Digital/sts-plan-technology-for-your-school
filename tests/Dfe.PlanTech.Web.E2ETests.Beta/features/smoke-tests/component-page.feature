@smoke
@user-school
Feature: Contentful components custom page

  Background:
    Given I am on the Contentful components test page
    And I should see the page heading "Component Testing Page"

    Scenario: Rich text content renders correctly
      Then I should see a rich text paragraph with text "This is a rich text body component."
      And I should see bold rich text with text "This is rich text bold text"
      And I should see a hyperlink with text "This is a hyperlink text" and href "dummy-url"
      And I should see headings with the following texts:
        | This is heading 1 |
        | This is heading 2 |
        | This is heading 3 |
        | This is heading 4 |
        | This is heading 5 |
        | This is heading 6 |
      And I should see an unordered list with items:
        | Bullet point 1 |
        | Bullet point 2 |
        | Bullet point 3 |
        | Bullet point 4 |
        | Bullet point 5 |
      And I should see an ordered list with items:
        | Numbered list 1 |
        | Numbered list 2 |
        | Numbered list 4 |
        | Numbered list 5 |

      Scenario: Table component is rendered correctly
        Then I should see a table with column headings:
          | Column 1 |
          | Column 2 |
          | Column 3 |
        And I should see a table row with values:
          | Row 1 Column 1 | Row 1 Column 2 | Row 1 Column 3 |
        And I should see a table row with values:
          | Row 2 Column 1 | Row 2 Column 2 | Row 2 Column 3 |

      Scenario: Header component is rendered correctly
        Then I should see the large header "This is a header component"
      Scenario: Inset text renders correctly
        Then I should see inset text "This is some inset text."

      Scenario: Notification banner renders correctly
        Then I should see a notification banner with title "Important"
        And I should see notification banner text "This is a the content of a notification banner."
        And the notification banner should contain a hyperlink with text "This text is a link to another page." and href "/dummy-url"
        And the notification banner should contain bold text "This is notification banner bold text."

      Scenario: Attachment component is rendered correctly
        Then I should see an attachment titled "Testing - Attachment"
        And the attachment should have file type "XLSX" and size "51 KB"
        And the attachment should have last updated text "Last updated 10 January 2025"

      Scenario: Accordion Section 1 content renders correctly
        When I expand the accordion section "Testing - Component - Accordion Section 1"
        Then the accordion section "Testing - Component - Accordion Section 1" should contain text "Accordion section 1 content."
        And the accordion section "Testing - Component - Accordion Section 1" should contain text "This is some accordion text."
        And the accordion section "Testing - Component - Accordion Section 1" should contain bold text "This is some accordion bold text."
        And the accordion section "Testing - Component - Accordion Section 1" should contain a hyperlink with text "This is a accordion hyper link text" and href "/dummy-url"
        And the accordion section "Testing - Component - Accordion Section 1" should contain an unordered list with items:
          | Accordion Bullet point 1 |
          | Accordion Bullet point 2 |
          | Accordion Bullet point 3 |
        And the accordion section "Testing - Component - Accordion Section 1" should contain an ordered list with items:
          | Accordion Numbered list 1 |
          | Accordion Numbered list 2 |
          | Accordion Numbered list 3 |

      Scenario: Accordion Section 2 content renders correctly
        When I expand the accordion section "Testing - Component - Accordion Section 2"
        Then the accordion section "Testing - Component - Accordion Section 2" should contain text "Accordion section 2 content."
        And the accordion section "Testing - Component - Accordion Section 2" should contain bold text "This is some accordion bold text."
        And the accordion section "Testing - Component - Accordion Section 2" should contain a hyperlink with text "This is a accordion hyper link text" and href "/dummy-url"
        And the accordion section "Testing - Component - Accordion Section 2" should contain an unordered list with items:
          | Accordion Bullet point 1 |
          | Accordion Bullet point 2 |
          | Accordion Bullet point 3 |
        And the accordion section "Testing - Component - Accordion Section 2" should contain an ordered list with items:
          | Accordion Numbered list 1 |
          | Accordion Numbered list 2 |
          | Accordion Numbered list 3 |
