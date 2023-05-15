# 0018 - Project And Namespace Naming Convention

* **Status**: accepted

## Context and Problem Statement
What should be the agreed naming convention for the .Net projects?  This also impacts the namespace naming convention.

## Decision Drivers
* SLT Lead Tech (Mamood Suyltan) wanting consistency across DfE projects
* How projects have been implemented within other DfE projects

## Considered Options
* `Sts.PlanTech.{name}`
* `Dfe.PlanTech.{name}`

## Decision Outcome

As the team name Sts (Schools Technology Service) could change in the future we've settled on using `Dfe.PlanTech.{name}`