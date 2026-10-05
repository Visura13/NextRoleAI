namespace NextRoleAI.Application.Recommendations;

public interface IJobMatchScorer
{
    string AlgorithmVersion { get; }

    MatchScore Score(CandidateMatchProfile candidate, JobMatchInput job);
}
