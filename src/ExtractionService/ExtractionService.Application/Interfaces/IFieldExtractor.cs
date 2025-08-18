using CaptureSys.ExtractionService.Domain.Entities;
using CaptureSys.Shared.Results;

namespace CaptureSys.ExtractionService.Application.Interfaces;

public interface IFieldExtractor
{
    Task<Result<ExtractionResult>> ExtractFieldsAsync(Guid documentId, string documentType, string extractedText);
    Task<Result<ExtractionResult>> ExtractFieldsWithTemplateAsync(Guid documentId, string templateName, string extractedText);
    Task<Result<List<ExtractionTemplate>>> GetTemplatesForDocumentTypeAsync(string documentType);
    Task<Result<ExtractionTemplate>> CreateTemplateAsync(string name, string documentType, string description, List<FieldExtractionRule> rules);
    Task<Result<bool>> UpdateTemplateAsync(Guid templateId, string name, string description, List<FieldExtractionRule> rules);
    Task<Result<bool>> DeleteTemplateAsync(Guid templateId);
    Task<Result<ExtractionResult>> ValidateExtractionAsync(Guid extractionId, string validatedBy);
}
