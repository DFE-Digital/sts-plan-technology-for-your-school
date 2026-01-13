# 0037 - Content Validation Process

* **Status**: proposed

## Context and Problem Statement

Certain fields in Contentful are mandatory, and we currently do not save any content items which are invalid, due to missing such fields.
This poses a problem with the following content creation process:
1. A question is created and details filled in
2. A new answer is added to that question
3. The answer is incomplete, so invalid, so doesn't save
4. Therefore no question -> answer relationship is saved
5. The answer is filled in
6. It then gets saved to the db on its own
7. the question -> answer relationship was never re-attempted, so still does not exist
8. The question then doesn't show this answer in production

This can be resolved by republishing the question, but it is frustrating for content designers to have to repeatedly republish content.
It also leaves production missing content until someone notices, so we need a way to fix this issue.

## Decision Drivers

- Ease of development
- Reliability of the solution
- How testable the solution is
- Minimising risk of error for the function
- Minimising risk of error in the web app

## Considered Options

### Overnight/periodic job to run a content validator and retrieve missing items:

- Pros
    - Minimal regression risk to web app as it can stay exactly as is
    - Can handle any error that has occured with the function, not just this, including it temporarily going down entirely
- Cons
    - In the time between an issue arising and the periodic job running, the app could be broken due to missing relationships, so this may not be adequate
    - May be resource intensive if run frequently, as it does full validation
    - Requires developing a solution to fix the issues found by the validator and setting up periodic jobs

### Making all database fields (except primary keys) nullable:

- Pros
    - Robust approach to the problem, it's highly likely to ensure the correct relationships save to the database every time
    - Minimal risk of errors within the Azure function
- Cons
    - High risk of hitting an error page in the web app when partial content exists, due to it expecting content that isn't there
    - Would require extensive testing with partially complete content items to be sure of the web app not breaking
    - Widespread changes within the code base, risk of regression

### Having default values for simple fields, and nullable content references
Not every field can have a default, for instance a question section requires an interstitial page, which cannot be populated with a default value,
but we could make every field that can have a default, have a default and the others nullable.

- Pros
    - Lower risk than making all fields nullable, as it requires fewer changes to the web app
    - Also a robust approach that is highly likely to work
    - Relatively low risk of errors within the Azure function
- Cons
    - May still be quite a few code changes involved in making all content references nullable, with the same risks as the option above (but less widespread)
    - Needs careful testing of all types which are made nullable
    - The defaults will render incomplete content "valid" when it isn't. This risk is small because Contentful doesn't allow publishing content with missing mandatory fields. So only UsePreview environments have a risk of showing blank content.

## Decision Outcome

Option 3. It's lowest risk and involvement whilst effectively resolving the problem. The suggested implementation will be to modify the way the Azure function maps a json object into database entities and add defaults at this stage, to minimise the changes required.
