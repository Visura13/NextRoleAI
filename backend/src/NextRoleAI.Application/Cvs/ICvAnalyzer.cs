namespace NextRoleAI.Application.Cvs;

public interface ICvAnalyzer
{
    Task<CvAnalysis> AnalyzeAsync(
        string extractedText,
        CancellationToken cancellationToken = default);
}
