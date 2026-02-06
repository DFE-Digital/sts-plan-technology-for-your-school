# 0031 - CMS DB Structure

- **Status**: accepted

## Context and Problem Statement

How do we store our CMS data in DB? Schemas?

## Decision Drivers

- Development ease
- Speed/effeciency

## Considered Options

### Singular Table

We could store all of our differnet content models in a single SQL table, by using JSON.

The rough strucure could be something like:

- Id (Contentful ID)
- Content model type
- JSON data (entire content as JSON)

#### Pros

- Quick development: there are minimal DB changes, and the coding for integration would be much simpler.
- Simplicity: this approach would require less C# classes, and therefore would be simpler to develop.

#### Cons

- Whilst MSSQL supports JSON columns, the reading from them is extremely slow.
  - This can be mitigated by things such as indexes, but this would likely require multiple for various content models. At this point, this approach becomes more questionable

#### Multiple Tables

We could store each content model in its own table.

#### Pros

- Designed for RDBMS: this approach aligns with the principles of a relational database management system.
- Greater efficiency: each table can be optimized for its specific content model, making queries and updates faster and more efficient.
- More clarity of purpose: the purpose of each table would be clear, making the database schema easier to understand and maintain.

#### Cons

- Increased development time: creating multiple tables would require more development time. Both in terms of the DB (schemas etc.), but also in terms of how we integrate it. We would have to amend some/all of our content model classes, if not create new ones. There would likely need to be mapping between classes, etc..

## Decision Outcome

After considering the pros and cons of each solution, we have decided to use multiple tables to store our CMS data in the database. While this approach requires more development time, it is more efficient and aligns with the principles of a relational database management system. Each table can be optimized for its specific content model, making queries and updates faster and more efficient, and the purpose of each table would be clear, making the database schema easier to understand and maintain.
