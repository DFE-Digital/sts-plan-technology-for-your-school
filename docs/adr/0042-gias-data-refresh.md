# 0042 - GIAS Data Refresh

* **Status**: accepted

## Context and Problem Statement

Get Information about Schools (GIAS) is the authoritative source of information about schools in England.
Relevant to plan technology for your school: it provides data about establishments, establishment groups, and the links between them.

Plan technology for your school currently uses this mapping data to show the list of establishments that a logged in multi-academy trust (MAT) user is linked to via their MAT.

Currently, the GIAS data is static/manually updated and does not refresh automatically.

## Decision Drivers

- One user has already been in touch about incorrect mappings between their MAT and the schools that they are linked to - this has been resolved manually.
- Additional traffic will be coming from upcoming additional features and functionality provided to MAT users, making it more important that these links are correct.
- A working fix applied sooner is preferable to a perfect solution applied later.

## Considered Options

- **Option 1**: Continue with the current manual process of updating GIAS data.
  - Caveats:
    - mostly reactive, in response to user reports of incorrect mappings (has happened only once already, but this is likely to increase as more users are onboarded)
    - any proactive updates / human involvement in the process would take time away from development of new features and functionality
    - manual steps are prone to error/accidents
- **Option 2**: Implement a process to fetch this data "live" at the point of a MAT user logging in.
  - Caveats:
    - no existing / readily-available API to query GIAS data directly
    - potentially non-GIAS data sources are available but would require further investigation to determine if they are suitable and reliable (agreement on SLAs etc.)
    - this is a potentially non-trivial change and would take time/effort to investigate and implement
    - consideration would be needed re: this introducing _a new hard-dependency_ on a third-party that must always be available for this service to run (mitigations such as caching would be needed)
- **Option 3**: Implement an automated process to refresh GIAS data periodically at a set interval (e.g., daily, weekly) and have this stored/cached/referenced within the service
  - (3a) begin to use some pre-existing python code that fetches GIAS data and updates the database.
    - Caveats:
      - this is written in Python ([not an approved/supported DfE technology](https://technical-guidance.education.gov.uk/guides/default-technology-stack/))
      - it is described/handed over as a "proof of concept", but _appears to be_ functional (work required to test/verify this)
      - would rely on GitHub Actions to run the scheduled task as opposed to being integrated into the service itself
      - little-to-no error handling or sanity checks of the data being fetched (risk of accidentally importing corrupted data)
      - no existing tests for this code
  - (3b) write new code to fetch the data from GIAS and store it in the database.
    - Caveats:
      - no prior-art in plan technology for your school to run scheduled tasks (GitHub Actions are currently in place/used for scheduled tasks)
      - there is no concept within the service of "privileged users" or a backoffice/admin interface within the service currently,
        which would be needed for DfE personnel to track the status of the GIAS data refresh process, or to trigger it manually (currently provided via GitHub Actions)


## Decision Outcome

Given timings and desire to have something functional sooner rather than later,
in addition to the absence of a readily-available API to query GIAS data
and despite caveats about future maintainability concerns,
we have chosen to implement option (3a) and make minimal changes to get the previous PoC code functioning and run on a schedule.

Future work is on the backlog to investigate further improvements and optimisations (e.g. options 2/3b/others).

