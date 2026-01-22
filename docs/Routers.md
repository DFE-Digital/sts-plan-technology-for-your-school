# Routing

Most routing is handled using conventional C# attributes. However, certain routes use extra classes/logic to ensure that the URL that is attempted to be loaded is a valid URL for the current stage of the establishment's user journey.

These classes are currently located in the [/src/Dfe.PlanTech.Web/Routing](/src/Dfe.PlanTech.Web/Routing) folder.

They are split into three main types:

1. `UserJourneyStatusProcessor` - this class loads shared information about the current user journey for the current section. E.g. whether it is completed or not, the next question in their journey, etc.
2. `UserJourneyStatusChecker`s - this class (and the implementations) are used by the above to determine the current status of a section for an establishment, and set various fields in the above `UserJourneyStatusProcessor` where appropriate
3. x`Router`s - these classes handle specific routing, page loading, etc. for actions. They validate that the action that is attempted to be navigated to is acceptable for the establishment's current status in the given section, and load the appropriate page if so. Otherwise, they redirect the user to a different page.

```mermaid
flowchart TD;
    AA[Router for action] --> A[UserJourneyStatusProcessor]
    subgraph UserJourneyStatusProcessor
        A[Get section submission status for establishment] --> B[Loop through status checkers]
        B --> C[Does the current status match this checker?]
        C -- Yes --> D[Set values in UserJourneyStatusProcessor]
        C -- No --> B
    end
    D --> E[Validate user journey status vs action]
    E --> F[Is valid URL/action for user journey?]
    F -- Yes --> G[Display page for action]
    F -- No --> H[Redirect to appropriate action]
```

## Routing Logic

```mermaid
graph TD
    subgraph CheckAnswers
        A[User navigates to check answer page] --> B{Is section on check answers status?}
        B -- Yes --> C[Show check answers page]
        B -- No --> D{Is the section completed?}
        D -- Yes --> E[Show self-assessment page]
        D -- No --> F{Is section in progress?}
        F -- Yes --> G[Show next question for section]
        F -- No --> E
    end
```

```mermaid
graph TD
    subgraph Recommendation
        A[User navigates to a recommendation page] --> B{Is the section complete?}
        B -- Yes --> C{Is it the recommendation for their maturity?}
        B -- No --> I
        C -- Yes --> D[Show recommendation page]
        C -- No --> E[Redirect to correct recommendation page]
        E --> D
        C -- No --> F{Is section on check answers status?}
        F -- Yes --> G[Show check answers page]
        F -- No --> H{Is section started?}
        H -- No --> I[Show self-assessment page]
        H -- Yes --> J[Show next question for user journey]
    end
```

```mermaid
graph TD
    subgraph Question page
        A[User navigates to a question page] --> B{Is the section in progress?}
        B -- Yes --> C{Have they already answered this question in their latest journey?}
        C -- Yes --> D[Show question page]
        C -- No --> E{Are they on the check answers stage?}
        E -- Yes --> I[Show check answers page]
        E -- No --> J{Is it the next question in their journey?}
        J -- Yes --> D
        J -- No --> H[Redirect to first question in section]
        B -- No; section is complete or not started --> G{Is it the first question for the section?}
        G -- Yes --> D
        G -- No --> K[Redirect to interstitial page]
        H --> D
    end
```
