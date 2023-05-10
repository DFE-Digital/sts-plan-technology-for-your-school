# 0017 - Unit Test Project Name And Location

* **Status**: accepted

## Context and Problem Statement
Where should the unit test projects be called and where should the be located?

## Decision Drivers

## Considered Options
* Location
  * Located within the `/src` directory along side the project being tested 
  * Located within the `/tests` directory 
* Naming convention
  * `Dfe.PlanTech.{name}.Tests`
  * `Dfe.PlanTech.{name}.UnitTests`

## Decision Outcome

After discussions with the SLT Lead Tech (Mamood Suyltan) the prefed location is `/tests`, naming the project as `Dfe.PlanTech.{name}.UnitTests` making the purpose clear.  This directory will contain other types of unit tests.