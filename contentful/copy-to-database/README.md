# Contentful Questionnaire Extract

This pipeline downloads several content types (see [content-types.txt](./content-types.txt)) from
Contentful, stores them in SQL Server, and flattens the JSON into relational tables for analysis or
mapping.

> **Prerequisite: jq**
>
> This project’s Bash script (`get-content.sh`) requires [jq](https://stedolan.github.io/jq/), a
> lightweight command-line JSON processor.
>
> Install instructions:
>
> - **Windows (Git Bash / MINGW / WSL)**:  
>   Download from [GitHub releases](https://github.com/jqlang/jq/releases) (e.g. `jq-win64.exe`),
>   rename to `jq.exe`, and edit your environment variables (`PATH`) (e.g. `C:\Program Files\jq`).  
>   Or install via package manager:
>   - Chocolatey: `choco install jq`
>   - Scoop: `scoop install jq`
> - **Linux**:
>   - Debian/Ubuntu: `sudo apt-get install jq`
>   - RHEL/CentOS: `sudo yum install jq`
>   - Fedora: `sudo dnf install jq`
> - **macOS**:  
>   `brew install jq`
>
> Verify installation with:
>
> ```bash
> jq --version
> ```

## Setup

- Create a copy of `/sql/.env-template` and rename it to `/sql/.env`
- Populate the variables in `.env` with the corresponding values from Contentful
  - You may need to create an API key if you don't have one already

## Usage

The process has two main parts:

1. **Bash Script (`get-content.sh`)**
   - Calls the Contentful CDN API for each content type in [content-types.txt](./content-types.txt).
   - Uses pagination (`limit/skip`) to fetch _all_ entries, not just the first 100.
   - Base64-encodes the full JSON responses for safe SQL insertion.
   - Generates a SQL script `1.insert-content-yyyyMMdd.sql` which can be run in SQL Server.

2. **SQL Script (`1.insert-content-yyyyMMdd.sql`)**
   - Creates a staging table `contentful.contentfulImport`.
   - Inserts the downloaded JSON blobs into this table.
   - Parses them into temp tables:
     - `#questions` → Question metadata, plus linked Answer IDs.
     - `#questionAnswers` → Normalised QuestionId → AnswerId mapping.
     - `#answers` → Answer metadata, plus optional next question link.
     - `#recommendationChunks` → Recommendation metadata, plus linked Answer IDs.
     - `#answerRecommendations` → Normalised RecommendationId → AnswerId mapping.
   - Provides a **full chain query** joining Question → Answer → Recommendation(s).

### Suggested Process

1. Run the Bash script

   ```bash
   .\get-content.sh
   ```

   This will generate `1.insert-content-yyyyMMdd.sql`.

2. Copy and paste the top 48 lines of the generated SQL into Azure DB / SSMS. Run those lines and
   check the data was decoded correctly.

3. Copy and paste the rest of the script. Run those lines.

4. (Optional): Run the script in [Full Chain Query](#full-chain-query) to check the data.

## Temp Table Structures

### `#questions`

| Column       | Description                       |
| ------------ | --------------------------------- |
| ContentfulId | Question entry ID                 |
| InternalName | Contentful internal name          |
| QuestionText | Question text (en-US)             |
| HelpText     | Help text (optional)              |
| Slug         | Slug field                        |
| AnswerIds    | Comma-separated linked answer IDs |

### `#questionAnswers`

| Column     | Description            |
| ---------- | ---------------------- |
| QuestionId | Question entry ID      |
| AnswerId   | Linked answer entry ID |

### `#answers`

| Column         | Description                       |
| -------------- | --------------------------------- |
| ContentfulId   | Answer entry ID                   |
| InternalName   | Contentful internal name          |
| AnswerText     | Answer text (en-US)               |
| NextQuestionId | Linked next question (if present) |

### `#recommendationChunks`

| Column       | Description                               |
| ------------ | ----------------------------------------- |
| ContentfulId | Recommendation entry ID                   |
| InternalName | Contentful internal name                  |
| Header       | Recommendation header                     |
| ContentValue | Recommendation content (en-US, long text) |
| AnswerIds    | Comma-separated linked answer IDs         |

### `#answerRecommendations`

| Column           | Description             |
| ---------------- | ----------------------- |
| RecommendationId | Recommendation entry ID |
| AnswerId         | Linked answer entry ID  |

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

- **Locale**: Currently assumes `en-US` is the default locale. If new locales are added, the parse
  logic may need adjustment.
- **Base64 encoding**: The JSON responses are encoded to handle special characters safely when
  inserting into SQL.
- **Reserved keywords**: `Content` field has been renamed to `ContentValue` to avoid conflicts with
  protected keywords in SQL.
- **Pagination**: The Bash function `get_contentful_entries` ensures all entries are pulled,
  regardless of count.
- **Staging table:**: The staging table `contentful.contentfulImport` is truncated on each run. If
  you need history, remove the DELETE line.
