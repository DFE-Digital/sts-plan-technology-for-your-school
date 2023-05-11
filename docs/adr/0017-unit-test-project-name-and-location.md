# 0017 - Unit Test Project Name And Location

* **Status**: accepted

## Context and Problem Statement
Where should the unit test projects be called and where should the be located?

## Decision Drivers
* SLT Lead Tech (Mamood Suyltan) wanting consistency across DfE projects
* How unit tests have been implemented within other DfE projecrts

## Considered Options
* Location
  * Located within the `/src` directory along side the project being tested 
  * Located within the `/tests` directory 
* Naming convention
  * `Dfe.PlanTech.{name}.Tests`
  * `Dfe.PlanTech.{name}.UnitTests`

## Decision Outcome

After discussions with the SLT Lead Tech (Mamood Suyltan) the preferred location is `/tests`, naming the project as `Dfe.PlanTech.{name}.UnitTests` making the purpose clear.  This directory will contain other types of unit tests.