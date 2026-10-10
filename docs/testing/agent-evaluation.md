# Agent evaluation

The evaluation separates deterministic safety/business checks from the quality of a recommendation. A run passes only when every mandatory rule passes and the Job Seeker explicitly approves it.

| Evaluation case | Expected result | Automated evidence |
|---|---|---|
| Normal objective | Four typed agent roles and bounded shortlist count | `PlanningAgent_CreatesFourDistinctRolesAndBoundedConstraints` |
| Golden candidate/job match | All validation rules pass | `ValidationAgent_AcceptsGoldenCaseAndRejectsUnexpectedTool` |
| Prompt injection | Workflow fails safely with no shortlist | `ObjectiveGuard_RejectsInstructionInjection` and PostgreSQL journey |
| Invalid objective shape/control character | Rejected before tool execution | `ObjectiveGuard_RejectsInvalidShape` |
| Unauthorized tool | `ToolAllowList` fails | golden/unsafe-tool test |
| Unconfirmed CV | `ConfirmedProfile` fails | `ValidationAgent_RejectsUnconfirmedDuplicateAndOutOfRangeProposal` |
| Duplicate job IDs | `UniquePublishedJobs` fails | same negative-case test |
| Score below 40 or above 100 | `ScoreRange` fails | same negative-case test |
| Cross-owner workflow access | `404 Not Found` | CV/recommendation integration journey |
| Publish without approval | No publish tool is available in pre-approval steps | orchestrator integration journey and stored tool-call audit |
| Tool/provider error | Sanitized safe failure; transient calls retry once | orchestrator implementation and workflow failure audit |

## Manual evaluation set

Run at least five repeatable scenarios against the deployed API: strong match, weak match, remote-only request, no suitable jobs, and injection attempt. For each, record workflow ID, objective, expected outcome, actual status, shortlisted IDs/scores, validation rules, approval state, step duration, algorithm/model version, and pass/fail. Use the same seeded data when comparing runs; model output may vary, so evaluate ranking relevance and evidence instead of claiming bit-for-bit reproducibility.

Never record hidden reasoning. The evidence is the structured plan, bounded tool calls, validation results, durations, approval decisions, and final state returned by the API.
