# 0028 - Command Line Arguments Parsing

- **Status**: accepted

## Context and Problem Statement

One of our projects ([DatabaseUpgrader](../../src/Dfe.PlanTech.DatabaseUpgrader/)) requires several command line arguments for its execution. What is the best method to read this, so that we can use their values in the project's excecution?

## Decision Drivers

- Open-source
- Readability
- Ease of development

## Considered Options

### Assumed order

We can assume that each command line argument is given in a specific order, and then use that.

E.g. `dotnet run dbupgrader [SQL-CONNECTION-STRING] [ARGUMENT2] [ARGUMENT3]`

However this is hard to maintain, hard to document, and leads to problems if certain arguments are optional.

### Command Line Parser

[Command Line Parser](https://github.com/commandlineparser/commandline) is a FOSS project designed for parsing command line arguments, named, easily.

It is well used, well documented, and well supported.

## Decision Outcome

We are using Command Line Parser as it will allow us to achieve our objectives easily, whilst allowing arguments to be added/amended easily in the future without impacting existing usage.
