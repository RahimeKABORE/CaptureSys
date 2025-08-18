using CaptureSys.ExtractionService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExtractionService.Application.Interfaces;

public interface IExtractionEngine
{
    Task<Result<ExtractedField>> ExtractFieldAsync(string text, FieldExtractionRule rule);
    Task<Result<List<ExtractedField>>> ExtractFieldsAsync(string text, List<FieldExtractionRule> rules);
    Task<Result<string>> ExtractByRegexAsync(string text, string pattern);
    Task<Result<string>> ExtractByKeywordAsync(string text, List<string> keywords);
    Task<Result<string>> ExtractByPositionAsync(string text, BoundingBox searchArea);
    Task<Result<double>> CalculateConfidenceAsync(string extractedValue, FieldExtractionRule rule);
}
