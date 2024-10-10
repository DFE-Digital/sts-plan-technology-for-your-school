# Dfe.PlanTech.InfraStructure.Data

C# .NET Core shared library project that is responsible for concrete implementation of our SQL database functionality.

SQL queries are managed using EF Core as the ORM.

There should be very little logic in this project, as the [Dfe.PlanTech.Application](/src/Dfe.PlanTech.Application/) project should be the one responsible for almost all business logic.

We have two DbContexts in use:

**[PlanTechDbContext](PlanTechDbContext.cs)**; this manages anything related to _users specifically_. So the actual user + establishment tables, submitted answers, etc.

**[CmsDbContext](CmsDbContext.cs)**; this manages data from our CMS (Contentful) that we store in our database. So things like pages, any content a page has, etc.
-  Configuration for the DbContext model is done via various `IEntityTypeConfiguration` implementations, which are stored in the [EntityTypeConfigurations](./EntityTypeConfigurations) folder.

### Resources

#### EF Core

- [EF Core overview documentation](https://learn.microsoft.com/en-us/ef/core/) (Microsoft's official documentation)
- [How to create and configure an EF Core model](https://learn.microsoft.com/en-us/ef/core/modeling/) (Microsoft's official documentation)

#### Other

- [Our documentation on our CMS->DB process](/docs/cms/README.md)
