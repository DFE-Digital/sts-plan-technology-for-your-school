# Dynamic Page Validator Tests

## Instructions

1. Extract Contentful content using CLI
2. Rename file to `contentful.js`, and make the JSON an object called Contentful. E.g. `const contentful = { ... JSON}`;
3. Export that object
4. Place the `contentful.js` in `helpers` folder in the parent folder. E.g. `Dfe.PlanTech.Web.E2ETests\cypress\helpers\contentful.js`
5. Remove the `skip` command on line 14.

_Note: Will look into automating the Contentful export for ease_

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
- [ ] RecommendationIntro
- [ ] RecommendationChunk
- [ ] RecommendationSection
- [x] Section *
- [x] TextBody *
- [x] Titles
- [x] WarningComponent

* Not fully complete. See below for notes.

| Content Type | Status Notes                                                                                                                        |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------- |
| Section      | Haven't added functionality to test current section status                                                                          |
| TextBody     | Haven't added validations for all element types (e.g. hyperlinks, strong, italics, etc.), haven't validated classes for most things |

### Page routing validated

- [x] Sub-topic routing
- [x] Recommendation routing

### Other validated

- [x] Page authentication

### Other to do

- [x] Do not fail if one error; run entire tests for issues.
- [x] Split tests up; not just one `it` function
- [ ] Complete documentation:
   - [ ] Merge with documentation in `contentful-helpers`
- [x] Make a Node project for the `contentful-helpers` to allow re-use, and prevent the fact that the code is copied and pasted into two places
- [ ] Clear section status before each run

#### Integrate with CI/CD Pipeline

_Note: Out of scope of the current user story for this work_

- [ ] Export Contentful content automatically
- [ ] Run E2E test based off the exported data as part of our CI/CD workflow