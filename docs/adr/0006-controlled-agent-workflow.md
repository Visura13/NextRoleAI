# ADR 0006: Controlled typed agent workflow

- Status: accepted
- Date: 2026-10-06

## Context

NextRoleAI needs a demonstrable agentic workflow with multiple specialized agents, tool use, durable state, observable decisions, validation, and a human approval boundary. The confirmed profile and shared recommendation contract are consumed by React and Flutter and covered by automated tests. ADR 0007 later replaced exact-keyword recommendation scoring with validated AI semantic ranking while retaining this workflow's deterministic safety rules.

The options considered were:

1. Microsoft Agent Framework with a hosted language model;
2. a separate Python agent service such as LangGraph;
3. an explicit typed C# workflow inside the existing application.

## Decision

Implement the first controlled workflow as a typed C# orchestrator in `NextRoleAI.AgenticAI`. It coordinates four distinct agents:

- Planning Agent converts a bounded user objective into a structured plan;
- Candidate Profile Agent reads only the user's confirmed CV/profile through an allow-listed tool;
- Job Discovery Agent invokes the published-job ranking tool and produces a bounded shortlist;
- Validation & Safety Agent independently validates the objective, plan, profile state, shortlist, and observed tool calls.

The workflow is an explicit state machine persisted in PostgreSQL. Each step records its role, status, duration, retry count, structured result summary, and tool calls. Tool execution uses typed input/output contracts, a fixed allow-list, a ten-second timeout, and one retry. Failures move the workflow to a safe terminal state.

Publishing the proposed shortlist is a high-impact action. It cannot occur until the Job Seeker explicitly approves it. Rejection publishes nothing. A revision request starts a new cycle and repeats every agent and validation rule.

No hidden chain-of-thought or free-form model reasoning is persisted or shown. The audit record contains only bounded objectives, structured decisions, validation results, tool metadata, and user-visible summaries. A future model-backed planner can be introduced behind the same orchestration boundary without weakening deterministic validation or approval.

## Consequences

- Workflow orchestration and safety validation remain typed C# logic; its ranking tool now requires the configured AI provider described in ADR 0007.
- React and Flutter observe and control the same durable workflow through one API.
- Tests can reproduce the golden path, prompt-injection failures, and unexpected-tool rejection.
- The implementation demonstrates delegation, tool use, state transitions, validation, and human oversight rather than presenting ordinary ranking as an opaque AI call.
- Natural-language planning is intentionally narrow. A future model integration may improve objective interpretation, but it must retain the tool allow-list, structured contracts, validation gate, and human approval.
