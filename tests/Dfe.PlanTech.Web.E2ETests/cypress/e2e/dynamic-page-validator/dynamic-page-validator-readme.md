# Dynamic Page Validator Tests

## Instructions

1. When branching from development, a dummy version of `contentful-data.json` will be in the `\fixtures` folder. This file exists to ensure that pipeline tests will be skipped. 
2. Delete `contentful-data.json` to enable a fresh data file to be created when the DPV is run.
3. Before running Cypress, run `npm install` from `\Dfe.PlanTech.Web.E2ETests` to install the Contentful export processor.
4. Run Cypress as normal (using `npx cypress open --config "baseUrl=URL"` where URL is that listed in your `cypress.env.json` file) and select the dynamic page validator from the Specs when in Cypress.
5. Ensure that you do not commit a modified `contentful-data.json` file. If needed, update the index to ignore changes to this file using `git update-index --assume-unchanged cypress/fixtures/contentful-data.json`.

### Components content validated

- [x] Answer
- [x] ButtonWithEntryReference
- [x] ButtonWithLink
- [x] Category
- [ ] ComponentDropDown; not currently used anywhere so unable to test.
- [x] Headers
- [x] InsetText
- [x] NavigationLink
- [x] Question
- [x] RecommendationIntro 
- [x] RecommendationChunk *
- [x] RecommendationSection
- [x] Section *
- [x] TextBody *
- [x] Titles
- [x] WarningComponent

* Not fully complete. See below for notes.

| Content Type        | Status Notes                                                                                                                        |
| ------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| Section             | Not currently testing for 'Not started' and 'In progress' section statuses                                                          |
| TextBody            | Haven't added validations for all element types (e.g. hyperlinks, strong, italics, etc.), haven't validated classes for most things |
| RecommendationChunk | WIP to create paths to ensure all RecommendationChunks are presented for test as chunks are no longer determined by	 maturity	    |

### Page routing validated

- [x] Sub-topic routing
- [x] Recommendation routing
- [ ] Amending answers from the Check Answers page
- [ ] Returning to an In Progress assessment

### Other validated

- [x] Page authentication
- [ ] Recommendations printing

### Other to do

- [x] Do not fail if one error; run entire tests for issues.
- [ ] Complete/reset journey if failing during question/answer validation (leaving section on 'In progress' fails the url check in the next routed test on that section)
- [x] Split tests up; not just one `it` function
- [ ] Complete documentation:
   - [ ] Merge with documentation in `contentful-helpers`
- [x] Make a Node project for the `contentful-helpers` to allow re-use, and prevent the fact that the code is copied and pasted into two places
- [ ] Clear section status before each run
- [ ] Validate content order within a page
- [ ] Break up tests further to prevent skipping of future tests in a path if early errors will not affect later tests

#### Integrate with CI/CD Pipeline

- [ ] Export Contentful content automatically
- [ ] Run E2E test based off the exported data as part of our CI/CD workflow