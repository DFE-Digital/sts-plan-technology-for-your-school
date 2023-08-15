# 0016 Tools For Building Unit Tests

* **Status**: accepted

## Context and Problem Statement
Which frameworks can be adopted within the PTFYS that will aid the developer to create clear, concise unit tests quickly and help facilitate TDD?

## Decision Drivers
* Open source
* Within DfE’s Technical Guidance
* Moq is used by the following services
	* [Dfe.Academies.External](https://github.com/DFE-Digital/Dfe.Academies.External)
	* [find-a-tuition-partner](https://github.com/DFE-Digital/find-a-tuition-partner)
* AutoFixture can be used to is used by the following services to generate 
	* [amsd-casework](https://github.com/DFE-Digital/amsd-casework)
	* [prepare-academy-conversions](https://github.com/DFE-Digital/prepare-academy-conversions)

## Considered Options

Mocking
* Moq
* FakeItEasy
* Rhino Mocks
* JustMock
* Microsoft Fakes (Visual Studio Enterprise edition)

Test Data
* AutoFixture
* NBuilder
* Bogus

Assertions
* Fluent Assertions
* NFluent
* Shouldly
* MSTest/NUnit and xUnit built-in assertions

## Decision Outcome

We will use Moq and xUnit for our testing purposes, as these are the two most popular testing packages currently available in their respective domains.