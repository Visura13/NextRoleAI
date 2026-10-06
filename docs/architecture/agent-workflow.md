# Controlled agent workflow

```mermaid
sequenceDiagram
    actor User as Job Seeker
    participant Client as React / Flutter
    participant API as ASP.NET Core API
    participant Orchestrator
    participant Planner as Planning Agent
    participant Profile as Profile Agent
    participant Discovery as Discovery Agent
    participant Validator as Validation Agent
    participant DB as PostgreSQL

    User->>Client: Enter bounded job-search objective
    Client->>API: POST /api/agent-workflows
    API->>Orchestrator: Start owner-scoped workflow
    Orchestrator->>Planner: Normalize and plan objective
    Planner-->>Orchestrator: Four typed steps and tool allow-lists
    Orchestrator->>Profile: Read confirmed candidate profile
    Profile->>DB: candidate-profile.read
    DB-->>Profile: Structured profile
    Orchestrator->>Discovery: Rank published jobs
    Discovery->>DB: published-jobs.rank
    DB-->>Discovery: Structured ranked jobs
    Orchestrator->>Validator: Validate prompt, plan, profile, scores, IDs, tools
    Validator-->>Orchestrator: Deterministic rule results
    Orchestrator->>DB: Persist pending proposal and audit trail
    API-->>Client: PendingApproval workflow
    User->>Client: Review evidence and approve/reject/revise
    Client->>API: POST /decision
    alt approved and valid
        API->>Orchestrator: Publish approved shortlist
        Orchestrator->>DB: shortlist.publish + approval audit
        API-->>Client: Completed workflow
    else rejected or revised
        API->>DB: Persist decision or new revision
        API-->>Client: Rejected or revised workflow
    end
```

The four agents have separate responsibilities, but all tool execution remains server controlled. Objective text cannot select a tool. Tool inputs and outputs are JSON contracts, each call is timed and stored, transient failures are retried once, each step is bounded by a timeout, and no shortlist becomes visible as approved before the owner acts at the human gate.
