# 0034 - Entity Mapping

- **Status**: accepted

## Context and Problem Statement

Efficiently mapping data between Contentful and our database schemas is a critical aspect of our system's functionality. The challenge lies in determining the most effective method that aligns with open-source principles and adheres to development best practices, ensuring maintainability, usability, and reliability.

## Decision Drivers

- Open source:
  - The chosen solution should embrace open-source principles, fostering collaboration, transparency, and community contribution.

- Development best practices:
  - The selected approach must adhere to development best practices, ensuring maintainability, usability, and reliability of the overall system.

## Considered Options

Three primary options were evaluated in the decision-making process:

- Manual:
  - Creating manual mapping classes for each entity, mapping properties one by one. While providing explicit control, it may become cumbersome and less maintainable as the complexity of relationships increases.

- Reflection:
  - Utilizing reflection where possible to automatically map properties between classes. This approach can offer efficiency but might lack explicitness and control over complex mapping scenarios.

- AutoMapper:
  - Leveraging AutoMapper, a widely-used and open-source library designed for automatic object-to-object mapping. It provides a balance between automation and configurability, making it a popular choice for simplifying the mapping process.

## Decision Outcome

After thorough consideration, the decision was made to implement a custom solution within Azure Functions. This decision was driven by the specific challenges posed by transitioning from a NoSQL database to an SQL database, particularly in the context of handling relationships, especially many-to-many relationships.

The custom solution within Azure Functions allows for specialized handling of relationship mapping, accommodating the nuances that arise during the shift between different database paradigms. This becomes particularly crucial when the properties on both classes do not align perfectly due to the transformation from a NoSQL to an SQL database.

In addition to this, for the conversion from the database to Contentful/view models, the decision is to use AutoMapper. This choice strikes a balance between automation and configurability, aligning with development best practices while benefiting from the advantages of a widely-used open-source library. This dual approach ensures that the entity mapping strategy is tailored to handle the specific intricacies of our database schema transitions while maintaining efficiency and adherence to development best practices in other mapping scenarios.
