# Contentful Questionnaire Extract

This pipeline downloads a list of Contentful content types (see
[content-types.txt](./content-types.txt)), stores the JSON payloads in a staging table, and then
parses them into relational tables for analysis or mapping.

> **Prerequisite: curl, jq, and base64**
>
> The Bash script [get-content.sh](./get-content.sh) requires [jq](https://stedolan.github.io/jq/),
> [curl](https://curl.se/), and a base64 implementation.
>
> Install instructions:
>
> - **Windows (Git Bash / MINGW / WSL)**:
>   - Install [jq](https://github.com/jqlang/jq/releases) and ensure it is available on your PATH.
>   - Install curl if it is not already available.
>   - Git Bash typically includes base64 support.
> - **Linux**:
>   - Debian/Ubuntu: `sudo apt-get install curl jq coreutils`
>   - RHEL/CentOS: `sudo yum install curl jq coreutils`
>   - Fedora: `sudo dnf install curl jq coreutils`
> - **macOS**:
>   - `brew install curl jq`
>
> Verify installation with:
>
> ```bash
> curl --version
> jq --version
> base64 --version
> ```

## Setup

- Create a copy of [.env-template](./.env-template) and rename it to [.env](./.env).
- Populate the variables in [.env](./.env) with your Contentful values:
  - `CONTENTFUL_TOKEN`
  - `ENVIRONMENT`
  - `SPACE_ID`
- Add or remove content types in [content-types.txt](./content-types.txt). Put one content type per
  line; comments after `#` are ignored.

## Files

- [get-content.sh](./get-content.sh) downloads the configured entries from Contentful and writes a
  SQL file containing the imported payloads.
- [content-types.txt](./content-types.txt) defines which Contentful content types to extract.
- [2.create-tables.sql](./2.create-tables.sql) creates the relational tables in the `contentful`
  schema.
- [3.populate-tables.sql](./3.populate-tables.sql) parses the staged JSON and populates those
  tables.

## Usage

The process has four main steps:

1. **Download the Contentful data**
   - Run [get-content.sh](./get-content.sh).
   - The script reads the values from [.env](./.env) and the content types from
     [content-types.txt](./content-types.txt).
   - It calls the Contentful CDN API for each content type, follows pagination to retrieve all
     entries, and base64-encodes the JSON payloads.
   - It generates a SQL file named `1.insert-content-yyyyMMdd.sql` in the current folder.

2. **Load the staged JSON**
   - Run the generated SQL file in Azure SQL or SQL Server Management Studio.
   - This creates and clears the staging table `contentful.contentfulImport`.
   - The script inserts the downloaded payloads as base64-decoded JSON into the `JsonResponse`
     column.

3. **Create the target tables**
   - Run [2.create-tables.sql](./2.create-tables.sql).
   - This drops and recreates the relational tables used for the extract.

4. **Populate the tables**
   - Run [3.populate-tables.sql](./3.populate-tables.sql).
   - This reads the latest JSON from `contentful.contentfulImport` for each content type and inserts
     the parsed values into the relational tables.

5. **_Optional_**
   - Run the script in [Full Chain Query](#full-chain-query) to check the data.

## Output tables

The scripts create and populate tables such as:

- `contentful.category`
- `contentful.page`
- `contentful.title`
- `contentful.section`
- `contentful.question`
- `contentful.answer`
- `contentful.recommendation`
- `contentful.textBody`

They also create join tables such as `contentful.categorySection`, `contentful.questionAnswer`,
`contentful.sectionQuestion`, `contentful.sectionRecommendation`, and
`contentful.recommendationAnswerStatus`.

## Full Chain Query

The generated SQL includes a query that joins everything together:

```sql
SELECT
    q.ContentfulId     AS QuestionId,
    q.QuestionText,
    a.ContentfulId     AS AnswerId,
    a.AnswerText,
    r.ContentfulId     AS RecommendationId,
    r.Header           AS RecommendationHeader
FROM #questions q
JOIN #questionAnswers qa
    ON q.ContentfulId = qa.QuestionId
JOIN #answers a
    ON qa.AnswerId = a.ContentfulId
JOIN #answerRecommendations ra
    ON a.ContentfulId = ra.AnswerId
JOIN #recommendationChunks r
    ON ra.RecommendationId = r.ContentfulId
ORDER BY
    q.InternalName,
    a.InternalName;
```

This produces one row per Question + Answer, with all linked recommendations aggregated.

## Notes

- Base64 encoding is used to handle special characters safely when inserting into SQL.
- The import script uses the most recent row for each content type from
  `contentful.contentfulImport`, so re-running the populate script after a fresh import will use the
  latest payloads.
- The tables are recreated each time by [2.create-tables.sql](./2.create-tables.sql), so any earlier
  data will be removed.
- The staging table is cleared each time the generated SQL file is run, so if you need history you
  should remove the delete step from the script.
