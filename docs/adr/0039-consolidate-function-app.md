# 0039 - Consolidation of Azure Function into the Web App

* **Status**: accepted

## Context and Problem Statement

Currently, we have two web services deployed in Azure: an Azure Function App with two functions and an Azure Container App using ASP.NET Core MVC.
The goal is to assess the feasibility of consolidating the functionality of the Azure Function into the MVC web app to streamline our architecture and reduce infrastructure overhead while maintaining the same functionality.

## Decision Drivers

- Simplification of deployment and management processes
- Reduction of infrastructure overhead
- Maintaining existing functionality
- Flexibility for future improvements and changes

## Considered Options

1. **Consolidate Azure Function into Web App**:
   - Create a new controller and action/route in the app to replace the function that receives webhook messages
   - Implement a background service in the app to replace the second function that reads messages from the queue and writes to DB

2. **Maintain Current Architecture**:
   - Keep the Azure Function and MVC web app as separate services without making any changes.

## Decision Outcome
The decision is to proceed with the consolidation of the Azure Function into the ASP.NET Core MVC application by:
- Creating a new controller to handle payloads from the Contentful webhook and save data to an Azure Service Bus queue.
- Implementing a background service to read from the Azure Service Bus queue in batches, process the payloads, and save data to the database.
