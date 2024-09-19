# Contentful Export Processing

Classes/functions/etc to process exported Contentful entries for various purposes.

## Core Classes

### Data Mapper

The [data mapper](./src/contentful/export-processor/data-mapper.js) class is the main class.

It takes a Contentful export in its constuctor, then performs various data mapping functions.

1. It uses the content type definitions to know what content type(s) a content reference ID could be referring to
2. Using this, it then edits all content and, where there is a reference, replaces it with the actual referenced content (e.g. if a `question` has an `answers` field containing ["id1"], it'll find `answer` with id `id1` and edit the array to be `[{id:"id1", text: "answer text"}]`)
3. It maps each subtopic when the field `mappedSections` is called. This takes all the subtopics, gets paths through them for each question, paths for each maturity, what recommendation content would be retrieved for these, etc..

This is used in the test suite generator (see below), and also in the dynamic test suites in the E2E tests.

### Content Types

The key content types for the user journey process (e.g. section, question, recommendations, etc.) are under the [content-types](./src/contentful/export-processor/content-types) folder

These have bespoke classes for them to allow additional functionality, particularly around the calculating potential user journeys.

The main one of all of them is the [section](./src/contentful/export-processor/content-types/section.js) class, which contains all logic for things such as calculating possible user journey paths for that section.

## Data Tools

[data-tools.js](/src/contentful/export-processor/data-tools.js) exports data from Contentful, processes it in various ways, and has the ability to save various information. It can:

1. Generate test suites
2. Generate possible user journey paths
3. Save errors about possible Contentful content issues

### Usage

1. Copy the `.env.example` file in the root of the folder and name it `.env`.
2. Fill in the various values for the Contentful space, access token etc..
3. Run `npm install` to install dependencies.
4. Run `node data-tools` to generate the test suites
    - You can also use:
        1. `npm run generate-test-suites` to generate the test suites
        2. `npm run data-tools` to run the other data tools but not the test suites

#### Environment variables

| Name                      | Description                                                                                                                                            |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| MANAGEMENT_TOKEN          | Contentful content management access token. These are personal tokens which can be generated under the `CMA Tokens` section of the Contentful settings |
| DELIVERY_TOKEN            |                                                                                                                                                        |
| USE_PREVIEW               |                                                                                                                                                        |
| SPACE_ID                  |                                                                                                                                                        |
| ENVIRONMENT               |                                                                                                                                                        |
| SAVE_FILE                 |                                                                                                                                                        |
| EXPORT_ALL_PATHS          |                                                                                                                                                        |
| GENERATE_TEST_SUITES      |                                                                                                                                                        |
| EXPORT_USER_JOURNEY_PATHS |                                                                                                                                                        |
| OUTPUT_FILE_DIR           |                                                                                                                                                        |
| FUNCTION_APP_URL          | Webhook URL where to post content migrations to                                                                                                        |

#### CLI Arguments

Some variables can also be passed as CLI arguments.
CLI arguments will take precedence over environment variables.

| CLI Argument                | Description                                                             | Matching env variable       |
| --------------------------- | ----------------------------------------------------------------------- | --------------------------- |
| -ts, --test-suites          | Generate test suites                                                    | `GENERATE_TEST_SUITES`      |
| -uj, --export-user-journeys | Export all user journeys                                                | `EXPORT_USER_JOURNEY_PATHS` |
| -o, --output-dir            | Where to save the outputted data to                                     | `OUTPUT_FILE_DIR`           |
| -s, --save-all-journeys     | Save all user journeys, not just the minimal required for various paths | `EXPORT_ALL_PATHS`          |

### Generate Test Suites

The data-tools.js file can generate test suites using [generate-test-suites.js](./src/contentful/export-processor/generate-test-suites.js) generates test suites for each sub-topic based on the exported Contentful data.

To generate test suites, make sure `GENERATE_TEST_SUITES` is set to `"true"` in your `.env` file.

#### Created files

- `plan-tech-test-suite.csv` - The actual tests for a user to follow
- `plan-tech-test-suite-appendix.csv` - Data to verify that the test suites are correct (e.g. specific expected content)

#### Generated Tests

For each sub-topic, the following tests should be created:

1. Can the user navigate to the sub-topic?
2. Can the user answer questions for a sub-topic?
3. An error is generated when a user tries to submit a question without answering it
4. A user can leave a partially completed sub-topic and it shows as "In Progress" at the self-assessment page
5. A user can continue a partially completed sub-topic
6. A user can't navigate ahead of their user journey by navigating to the URL manually
7. After completing a user journey, they see a success modal at the top of page
8. A user can change their answers from the check answers page
9. A path for "Low" recommendations
10. A path for "Medium" recommendations
11. A path for "High" recommendations

### User Journey Paths

Data-tools.js can also generate and save possible user journeys for each sub-topic using the [write-user-journey-paths.js](./src/contentful/export-processor/write-user-journey-paths.js) file.

To export the journey paths, make sure `EXPORT_USER_JOURNEY_PATHS` is set to `true` in your `.env` file.

By default, it will only export a minimal amount of data:

1. Per subtopic, it will create a `json` file containing:
  - Total number of possible user journeys through the sub-topic per maturity
  - One+ user journey path required to navigate through all questions in the sub-topic
  - One user journey per maturity rating
2. If `EXPORT_ALL_PATHS` is set to `true` it will save _all_ possible user journeys.

### Errors

Finally, the data-tools.js file will write a `contentful-errors.md` file containing various errors, if any, encountered during the Contentful mapping. This includes:

1. Missing data that is referenced
2. Maturity ratings without a possible user journey in a subtopic
3. Questions in a sub-topic that have no path to them.
4. Recommendation chunks with no answers attached to them

## Data Migrator

1. Uses Data Mapper class
2. Uses Contentful API to fetch data from Contentful
3. Gets content and calculates how many outgoing references it has, and how many incoming references it has
4. Gets all content with 0 outgoing references
5. POSTs to webhook
6. Updates content with references changes
7. Repeat step 4-6 until all content has been posted

### Usage

1. Setup `.env` (copy `.env.example` and setup fields as necessary)
2. Run `npm install`
3. Run `node data-migrator.js`

### Required Environment variables

MANAGEMENT_TOKEN
DELIVERY_TOKEN
USE_PREVIEW
SPACE_ID
ENVIRONMENT
FUNCTION_APP_URL