# 0032 - Classes/Code Structure for DB Entities

- **Status**: accepted

## Context and Problem Statement

How do we model our C# classes for reading/writing CMS models?

## Decision Drivers

- Development ease: how easy it is to develop and maintain the C# classes for the CMS and database.
- Speed/efficiency: how quickly and efficiently the database can be queried and updated.
- Best coding practices: following best practices for code maintainability and readability.

## Considered Options

### One class per content model

- We could simply amend our existing content models so that they are used for the CMS **and** for storing in the DB.

#### Pros

- Much quicker to develop

#### Cons

- Worse coding practices: this approach violates the Single Responsibility Principle, as the same class is responsible for both the CMS and the database.
  - This can lead to code that is less maintainable, less extensible, and harder to understand.
  - Changes to the database schema can also have a ripple effect on the CMS (and vice versa).,

### Multiple classes for each content model

- For each content model:
  - One class for CMS
  - One class for DB
  - As many view model classes as needed

#### Pros

- Best coding practice: this approach follows best practices for code maintainability and readability, as each class has a single responsibility.
- Easier to read: it would be easier to understand the code, as each class would have a clear purpose.
- Easier to understand: it would be easier to understand how the CMS and database interact with each other.
- More maintainable: this approach would be more maintainable in the long run, as changes to the database schema would not affect the CMS.

#### Cons

- Increased development time: creating multiple classes for each content model would require more development time.
- A lot more code changes: this approach would require more code changes, as there would be more classes involved.
- A lot of class mapping: this approach would require class mapping, which could be time-consuming.

## Decision Outcome

We are using 2 classes for each content type; one for Contentful, and one for the database. We use interfaces where possible to ensure that they match as closely as possible, to ensure that mapping and using them interchangably is as easy as possible.
