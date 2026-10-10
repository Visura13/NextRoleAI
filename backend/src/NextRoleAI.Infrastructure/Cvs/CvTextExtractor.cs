using DocumentFormat.OpenXml.Packaging;
using NextRoleAI.Application.Cvs;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace NextRoleAI.Infrastructure.Cvs;

internal sealed class CvTextExtractor : ICvTextExtractor
{
    public bool Supports(string extension) =>
        extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        content.Position = 0;
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => Task.FromResult(ExtractPdf(content, cancellationToken)),
            ".docx" => Task.FromResult(ExtractDocx(content)),
            _ => throw new NotSupportedException("Only PDF and DOCX CVs are supported.")
        };
    }

    private static string ExtractPdf(Stream content, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(content);
        var pages = new List<string>();
        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            pages.Add(ContentOrderTextExtractor.GetText(page));
        }

        return string.Join(Environment.NewLine, pages);
    }

    private static string ExtractDocx(Stream content)
    {
        using var document = WordprocessingDocument.Open(content, false);
        return document.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
    }
}
