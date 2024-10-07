# 0040 - SQL Schema Improvements

* **Status**: proposed

## Context and Problem Statement

Following the beta phase, to ensure efficiency of our database, we are doing a review of our existing schema, seeing how
well it is working and considering improvements we may be able to make.

## Decision Drivers

- Memory usage of the querying the database
- Data storage and not storing more than is required
- Having an efficient schema that is easy to use and work with

## Considered options

Options looked into that don't form part of the initial recommended actions off this spike.

- Changing stored procedures such as `GetSectionStatuses` to views
    - However on testing, this had negligible difference in performance
- Contentful Schema changes
    - Delete `SubtopicRecommendations` table
    - Delete `RecommendationSections` table as it ultimately holds the same information as `Sections`
    - Delete `SubtopicRecommendationIntros` table
        - Its a link table, but the recommendation intros never get shared between sections
        - So instead, `RecommendationIntros` can have a `SectionId`
    - **This would need input from Content Designers so the same changes can be mirrored in Contentful, currently, the
      purpose of this structure is for ordering the recommendation chunks, but if we were able to do that based off the
      question, this would potentially not be required. A future suggestion**
    - A smaller change, that preserves ordering would be to merge the `RecommendationSections` and `SubtopicRecommendationIntros` tables
        - This would make the naming clearer, and preserve ordering
- Schema simplification
    - The `Contentful.RichTextDataDbEntity` table could be removed and the `Uri` placed directly on the
      `RichTextContent` table, as its often empty, and rarely big, so little is saved from it being in a separate table
- Improving the performance of fetching page contents
    - This is the slowest query we have because the structure of the schema requires it to join to every table. However it is still fast, and as it is cached, it doesn't seem worthwhile to overhaul the schema to improve it, as it won't make much difference.

Schema change suggestions in a diagram:
blue is tables we could change, red tables to delete (displayed with two different layouts, content is identical)

| Hierarchic layout                                                            | Radial layout                                                        |
|------------------------------------------------------------------------------|----------------------------------------------------------------------|
| ![hierarchic layout](/docs/assets/adr_diagrams/adr_40_hierarchic_layout.svg) | ![radial layout](/docs/assets/adr_diagrams/adr_40_radial_layout.svg) |

## Recommendations

### Contentful schema

- Change `GetCurrentSubmissionId` to a UDF
    - It calculates one value and outputs it, which is what UDFs are perfect for and will simplify existing usage of it
- Add a dateCreated and dateLastUpdated column to the `ContentComponents` table.
    - This is useful for developers to see when content has changed, and makes it easier to debug content issues.
- Add missing foreign keys
    - `Contentful.Buttons` PK -> `ContentComponents`
    - `Contentful.ButtonWithEntryReferences` PK -> `ContentComponents`
    - `Contentful.ButtonsWithLinks` PK -> `ContentComponents`
    - `Contentful.NavigationLink` PK -> `ContentComponents`
    - `Contentful.InsetTexts` PK -> `ContentComponents`
    - `Contentful.ComponentDropDowns` PK -> `ContentComponents`
- For content types with short text fields, alter these columns in the database to be `nvarchar(256)`
    - We don't need them all to be `nvarchar(max)` and this change might result in a small improvement in memory
      required for query execution
- Add the column `InternalName` to all content tables:
    - It currently only exists on `Contentful.Categories` and `Contentful.Pages`. Some current tables are made up
      entirely of ids pointing to other tables, which makes it quite difficult to look up and debug content during
      development. The `InternalNames` are generally very descriptive so would be a useful tool to have included in the
      data

### dbo schema

- Alter the `SubmitAnswer` procedure to reduce data duplication
    - Currently every time an answer is submitted it puts a new record into the `questions` and `answers` tables with
      the question and answer text
    - The text doesn't change that frequently so this could lead to large amounts of duplication
    - Instead we should lookup the question and answer text by contentful reference, and only insert a new record if it
      doesn't already exist or has changed
- Write a datafix to do this for existing records, find all duplicate (ref, text) pairs, point responses at the first
  duplicate, and delete the others.
- Add an index to the `response` table on `submissionId` as this is frequently used to connect the two tables

## Decision Outcome
