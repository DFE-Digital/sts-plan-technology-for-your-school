# Dynamic Page Validator Tests

## Instructions

1. Extract Contentful content using CLI
2. Rename file to `contentful.mjs`, and make the JSON an object called Contentful. E.g. `const contentful = { ... JSON}`;
3. Export that object
4. Place the `contentful.mjs` in the same folder as `dynamic-page-validator.cy.js`
5. Remove the `skip` command on line 14.


### Components content validated

- [ ] Answer
- [ ] ButtonWithEntryReference
- [x] ButtonWithLink
- [ ] Category
- [ ] ComponentDropDown
- [x] Headers
- [ ] InsetText
- [ ] NavigationLink
- [ ] Question
- [ ] RecommendationPage
- [ ] Section
- [x] TextBody *
- [x] Titles
- [ ] WarningComponent

* Haven't completed; missing certain validations.

### Page routing validated

- [ ] Sub-topic routing
- [ ] Recommendation routing

### Other validated

- [x] Page authentication

### Other to do

- [ ] Do not fail if one error; run entire tests for issues.
- [ ] Split tests up; not just one `it` function

#### Integrate with CI/CD Pipeline

_Note: Out of scope of the current user story for this work_

- [ ] Export Contentful content automatically
- [ ] Run E2E test based off the exported data as part of our CI/CD workflow