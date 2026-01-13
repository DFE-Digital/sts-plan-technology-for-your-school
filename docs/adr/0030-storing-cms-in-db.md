# 0030 - Storing CMS Data In Database

* **Status**: accepted

## Context and Problem Statement

The context of this decision is whether to store CMS data in a database and if so, how to do it effectively. 

## Decision Drivers

- Cost: implementing changes would require non-minor development time, and the required Azure resources would have a cost impact (although this would be minor).
- Speed: moving to a database-only approach would be faster as the database will be hosted in the same environment as the web app.
- Efficiency: improving request efficiency and implementing caching would greatly enhance the performance of the system.
- Development: more infrastructure components means more points of failure.
  
## Considered Options

### Storing Data in the Database

#### Pros

- Improving request efficiency and implementing caching would greatly enhance the performance of the system.
- Moving to a database-only approach would be faster as the database will be hosted in the same environment as the web app.
- Will remove possible Contentful rate limit that we might hit in the next phase of the service

#### Cons

- Implementing the changes would require non-minor development time
- The required Azure resources would have a cost impact (although this would be minor)
- More infrastructure components means more points of failure.

### Solutions

#### API Controller(s) + Route(s) in Existing App

We could create controller(s) with route(s) in our existing web app, that would simply write the data to the database when received.

##### Pros

- No extra infrastructure
- Quicker development

#### Cons

- Single point of failure: if the application fails, no data is written to the database.
- Harder to handle errors: if an error occurs, it is harder to detect and fix.
- More likely chance of missing updates: if an update is missed, there is no way to recover it.

#### Azure Functions + Queue(s)

We could create new infrastructure in Azure that uses queue(s) to have an asynchronous approach instead.

##### Pros

- Resilient: if an error occurs, the system can recover automatically.
- Minimal extra cost: the cost is extremely small, especially when compared to the existing cost. Should be around £6 a month at most
- Easier to handle errors: errors can be detected and fixed more easily.
- No missing updates: all updates are processed by the system.

##### Cons

- Still some extra cost: resources still cost.
- More development time: it takes more time to set up the infrastructure and code the solution.
- More areas for development (Infrastructure as Code, etc.): additional areas of expertise may be required.

## Decision Outcome

After considering the pros and cons of each solution, we have decided to store our CMS data in the existing database using Azure Functions and Azure Service Bus (for queueing), and whatever other infrastructure is required. This approach will be more efficient and faster than the existing approach, and it will also remove the possible Contentful rate limit that we might hit in the next phase of the service.

By having an asynchronous process as architected, it minimizes the risk of data loss, as it reduces the chances of errors. Although this approach is slightly more complex and costly, it is worth the investment as it will improve the system's performance and efficiency.

