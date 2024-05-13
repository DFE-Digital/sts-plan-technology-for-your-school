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

## Generate Test Suites

[generate-test-suites.js](./src/contentful/export-processor/generate-test-suites.js) is a script that exports data from Contentful and generates test suites for that environment.

It uses the DataMapper class to map the content, then creates various "tests" based off the calculated data.

### Usage

1. Copy the `.env.example` file in the root of the folder and name it `.env`.
2. Run `npm install` to install dependencies.
4. Run `npm run generate-test-suites` to generate the test suites.
5. The script will output three files:

- `plan-tech-test-suite.csv` - The actual tests for a user to follow
- `plan-tech-test-suite-appendix.csv` - Data to verify that the test suites are correct (e.g. specific expected content)
- `content-errors.md` - Any errors that were found in the Contentful content

### Generates Tests

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
