# 0022 - Unit Test Reporting Within GitHub

- **Status**: accepted

## Context and Problem Statement

How can developers easily review unit test results within GitHub

## Decision Drivers

- Improve unit test result visibility

## Considered Options

- [Publish Test Results](https://github.com/marketplace/actions/publish-test-results)
  - Only shows unit test result counts
- [Test Reporter](https://github.com/marketplace/actions/test-reporter)
  - This gives a detailed breakdown of unit test results
  - Can be configured to only output failures when many unit tests are being run

## Decision Outcome

We'll implement **Test Reporter** as this provides a detailed report and can be configured to show only errors when it is reporting on many unit tests.
