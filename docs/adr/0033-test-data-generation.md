# 0033 - Unit Test Data

* **Status**: accepted

## Context and Problem Statement

Determining the optimal approach for creating, using, and generating data for unit testing.

## Decision Drivers

- Matches/covers test cases:
  - For scenarios requiring a comprehensive exploration of various data combinations, an automated approach was deemed advantageous. On the other hand, when the focus was on scrutinizing specific edge cases, such as null values, a manual method was considered more appropriate.
  - Example: If testing involves a multitude of diverse data combinations, an automated method like Bogus, capable of generating varied datasets, is preferable. Conversely, for scenarios where precision in handling specific edge cases is paramount, manual creation provides the necessary control.
- Ease of use when developing
- Speed of development
- DFE standards
  
## Considered Options

- Manual test case creation:
  - The traditional method of manually crafting test cases, providing explicit control over the data used for testing. Suitable for meticulous handling of specific conditions, but may prove time-consuming and less scalable for extensive test coverage.
- [Bogus](https://www.nuget.org/packages/Bogus):
  - A C# package that generates a variety of test data in a random manner with high customizability. It is the most widely used package of its type for C#, open source (thus aligning with DFE standards) making it a robust choice for automated data generation.

## Decision Outcome

After careful consideration, the decision was made to leverage both manual test case creation and the Bogus library. This hybrid approach capitalizes on the strengths of each method, allowing for a flexible and comprehensive strategy in unit test data management.

By combining manual creation for specific, targeted cases and Bogus for efficient and varied automated generation, the team can strike a balance that aligns with the unique requirements of each testing scenario. This decision promotes adaptability, ensuring that the testing process remains robust and responsive to the evolving needs of the software development lifecycle.