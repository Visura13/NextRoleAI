# NextRoleAI web

The role-aware React and TypeScript client for NextRoleAI. It uses Vite, React Router, and SWR and consumes the ASP.NET Core API.

Job Seekers can manage their profile, browse jobs, upload and confirm a CV, and inspect explainable ranked recommendations. Recruiters can manage their company and job postings.

```powershell
npm install
npm run dev
```

Available checks:

```powershell
npm run lint
npm test
npm run build
```

Copy `.env.example` to `.env.local` when the API is not available at the default `http://localhost:5251` address.
