namespace NextRoleAI.Application.Cvs;

public interface ICvProfileParser
{
    ExtractedCvProfile Parse(string text);
}
