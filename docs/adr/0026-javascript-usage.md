# 0026 - Back button functionality

* **Status**: accepted

## Context and Problem Statement

We have a "Back" link at the top of most of our pages. How do we best keep track of a user's navigation journey in our app, so that we can correctly navigate them backwards if they click on the link?

## Decision Drivers

* Accessibility
* Performance
* Existing DFE usage

## Considered Options

### Content Models

We could modify all the Contentful content models in such a way so that we know what the last page a user was on is likely to be. However:

- This is not guaranteed, as their journey may be slightly different each time
- This will require a lot of keeping track in the content, which is likely to cause mistakes and errors
- This will cause a lot of duplication in content models (where a content will need to reference the next page, but then the next page will also have to reference back)

### TempData

We could keep a track of the user's journey on the serverside, and use TempData to pass it back/forth each time the user navigates across pages.

However, the total size limit of cookies is 4096 bytes, which we might quickly get close to tracking just a few URLs. And, given that the server will be split across multiple instances, session management will be impossible without caching.

### Caching

We could cache the user's history server side, using something like SQL server (see [ADR 24 on caching](./0024-caching.md)). However, this will cause a great increase in SQL server requests for something relatively minor.

### JavaScript

Javascript will limit the amount of people who can use it (as people might have it disabled for example), however it keeps the data client side, minimises request data transfered, and minimises excessive requests from the server.


## Decision Outcome

From looking at other Gov/DFE websites, it appears most either hard code the back link directly, or use Javascript. Given the problems with the content model solution, Javascript was chosen as our best 

Javascript was chosen as it is the best use case for the project,