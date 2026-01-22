# 0014 - Executing Unit Tests

- **Status**: accepted

## Context and Problem Statement

What unit test runner should be adopted within the PTFYS that integrates well into Visual Studio, can be executed within pipeline, performs well and is open source and actively developed?

## Decision Drivers

- Open source
- Within DfE’s Technical Guidance
- xUnit is used by the following services
  - [find-a-tuition-partner](https://github.com/DFE-Digital/find-a-tuition-partner)
  - [fh-referral-api](https://github.com/DFE-Digital/fh-referral-api)
  - [prepare-academy-conversions](https://github.com/DFE-Digital/prepare-academy-conversions)
- NUnit is used by the following services
  - [amsd-casework](https://github.com/DFE-Digital/amsd-casework)
  - [Dfe.Academies.External](https://github.com/DFE-Digital/Dfe.Academies.External)

## Considered Options

- MSTest
- xUnit.net
- NUnit

## Decision Outcome

xUnit.net is the chosen framework as this is used by other DfE projects and the team are also familiar with this . this is a free, open source, community-focused unit testing tool for the .NET Framework. written by the original inventor of NUnit v2.
