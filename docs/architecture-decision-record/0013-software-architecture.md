# 0013 - Software Architecture

- **Status**: accepted

## Context and Problem Statement

What software architecture should be adopted within the PTFYS service to ensure we're following modern and existing DfE standards?

## Decision Drivers

- Within DfE’s Technical Guidance
- SLT Lead Tech (Mamood Suyltan) would like a modern architecture
- Clean Architecture has been used for other DfE Java services

## Considered Options

- Clean Architecture
  - Layer description
    - UI: MVC application
    - Application: Contains all application logic
    - Domain: Contain all entities, enums, exceptions, interfaces, types and logic specific to the domain layer.
    - Infrastructure: Contains classes for accessing external resources such as file systems, web services, smtp, and so on.
  - Reference
    - [Uncle Bob Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
    - [Microsoft Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)

- Clean Architecture with CQRS
  - CQRS
    - Pros
      - Scalability / Optimisation
      - Simplicity & maintainability
    - Cons
      - Increased complexity
      - Not suitable for all applications (e.g. small applications or applications with simple domains)
  - Jason Taylor: [Clean Architecture Solution Template](https://github.com/jasontaylordev/CleanArchitecture)
  - Matt Hewrenze: [clean-architecture-core](https://github.com/matthewrenze/clean-architecture-core)
- Hexagonal Architecture
- Onion Architecture
- Nth Tier Architecture

## Decision Outcome

We have decided to use Clean Architecture in our project. We are not using CQRS as it is unnecessarily complex for the scope of our MVS.
