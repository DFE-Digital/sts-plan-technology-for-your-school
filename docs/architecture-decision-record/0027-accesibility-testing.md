# 0027 - Accessibility Testing

- **Status**: accepted

## Context and Problem Statement

How do we best test the website for accessibility issues?

## Decision Drivers

- Ease of testing
- Reliability
- Existing DFE usage

## Considered Options

### Manual Testing

We could manually test the website, using tools such as [axe](https://www.deque.com/axe/), which is recommended by the Government & DFE.

### Automation Testing

We could automate the website testing in our end-to-end tests.

## Decision Outcome

We are using [cypress-axe](https://www.npmjs.com/package/cypress-axe) in our workflow for accessibility testing, and manual testing can be done seperately as/where needed.

This is a great fit for us given that:

1. We are already using Cypress
2. It is easy to test each page using the tool
3. Existing DFE projects use this solution
