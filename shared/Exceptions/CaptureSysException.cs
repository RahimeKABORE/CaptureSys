namespace CaptureSys.Shared.Exceptions;

/// <summary>
/// Exception de base du système CaptureSys
/// </summary>
public abstract class CaptureSysException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Details { get; }

    protected CaptureSysException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
        Details = new Dictionary<string, object>();
    }

    protected CaptureSysException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = new Dictionary<string, object>();
    }

    public CaptureSysException AddDetail(string key, object value)
    {
        Details[key] = value;
        return this;
    }
}

/// <summary>
/// Exception levée quand une entité n'est pas trouvée
/// </summary>
public class EntityNotFoundException : CaptureSysException
{
    public EntityNotFoundException(string entityType, Guid id) 
        : base("ENTITY_NOT_FOUND", $"{entityType} with ID {id} was not found")
    {
        AddDetail("EntityType", entityType);
        AddDetail("EntityId", id);
    }
}

/// <summary>
/// Exception levée lors d'erreurs de validation métier
/// </summary>
public class BusinessValidationException : CaptureSysException
{
    public List<string> ValidationErrors { get; }

    public BusinessValidationException(string message, List<string> validationErrors) 
        : base("BUSINESS_VALIDATION_ERROR", message)
    {
        ValidationErrors = validationErrors;
        AddDetail("ValidationErrors", validationErrors);
    }

    public BusinessValidationException(List<string> validationErrors) 
        : this("Business validation failed", validationErrors)
    {
    }
}

/// <summary>
/// Exception levée lors d'erreurs de traitement OCR
/// </summary>
public class OcrProcessingException : CaptureSysException
{
    public OcrProcessingException(string message) 
        : base("OCR_PROCESSING_ERROR", message)
    {
    }

    public OcrProcessingException(string message, Exception innerException) 
        : base("OCR_PROCESSING_ERROR", message, innerException)
    {
    }
}

/// <summary>
/// Exception levée lors d'erreurs de classification
/// </summary>
public class DocumentClassificationException : CaptureSysException
{
    public DocumentClassificationException(string message) 
        : base("DOCUMENT_CLASSIFICATION_ERROR", message)
    {
    }

    public DocumentClassificationException(string message, Exception innerException) 
        : base("DOCUMENT_CLASSIFICATION_ERROR", message, innerException)
    {
    }
}
