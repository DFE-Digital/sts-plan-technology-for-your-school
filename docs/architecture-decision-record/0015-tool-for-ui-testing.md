# 0015 - Tool For UI Testing

* **Status**: accepted

## Context and Problem Statement

What tool should be adopted within the PTFYS service to facilitate UI testing?

## Decision Drivers

* Within DfE’s Technical Guidance
* DfE projects using Cypress
	* [find-a-tuition-partner](https://github.com/DFE-Digital/find-a-tuition-partner)
	* [trams-data-api](https://github.com/DFE-Digital/trams-data-api)
  
## Considered Options

* Cypress
* Selenium / specflow
* Puppeteer

## Decision Outcome

Using [Cypress](https://cypress.io) as it is the most commonly used UI testing application/framework across DFE.