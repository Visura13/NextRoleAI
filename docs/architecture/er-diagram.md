# Entity relationship diagram

The diagram focuses on application-owned tables. ASP.NET Core Identity also creates its standard role, claim, login, token, and user-role tables around `AspNetUsers`.

```mermaid
erDiagram
    AspNetUsers ||--o{ RefreshTokens : owns
    AspNetUsers ||--o| JobSeekerProfiles : has
    JobSeekerProfiles ||--o{ JobSeekerSkills : contains
    AspNetUsers ||--o| CompanyProfiles : manages
    CompanyProfiles ||--o{ JobPostings : publishes
    JobPostings ||--o{ JobSkills : requires
    AspNetUsers ||--o| CvDocuments : uploads
    CvDocuments ||--o{ CvSkills : extracts
    AspNetUsers ||--o{ AgentWorkflowRuns : starts
    AgentWorkflowRuns ||--o{ AgentWorkflowSteps : contains
    AgentWorkflowSteps ||--o{ AgentToolCalls : records
    AgentWorkflowRuns ||--o{ AgentValidationResults : validates
    AgentWorkflowRuns ||--o{ AgentShortlistItems : proposes
    JobPostings ||--o{ AgentShortlistItems : ranks
    AgentWorkflowRuns ||--o{ AgentApprovalDecisions : records
    AspNetUsers ||--o{ JobApplications : submits
    JobPostings ||--o{ JobApplications : receives
    AgentWorkflowRuns o|--o{ JobApplications : sources
    JobApplications ||--o{ ApplicationStatusEvents : audits
    JobApplications ||--o{ NotificationDeliveries : triggers
    AspNetUsers ||--o{ NotificationDeliveries : receives
```

Important integrity rules include one profile and one CV per Job Seeker, one company per Recruiter, unique skills inside their parent, one application per seeker/job pair, unique shortlist rank and job within a workflow, indexed ownership/status queries, restricted deletion of referenced jobs, and cascading deletion only for true owned child records. EF Core migrations in `backend/src/NextRoleAI.Infrastructure/Persistence/Migrations` are the executable schema history.
