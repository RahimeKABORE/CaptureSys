namespace CaptureSys.Shared.Results;

/// <summary>
/// Représente le résultat d'une opération
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; protected set; }
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        if (!string.IsNullOrEmpty(error))
        {
            Errors.Add(error);
        }
    }

    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList();
        Error = Errors.FirstOrDefault();
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static implicit operator Result(string error) => Failure(error);
}

/// <summary>
/// Représente le résultat d'une opération avec une valeur de retour
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; protected set; }

    protected Result(bool isSuccess, T? value = default, string? error = null) 
        : base(isSuccess, error)
    {
        Value = value;
    }

    protected Result(bool isSuccess, T? value, IEnumerable<string> errors) 
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value);
    public static new Result<T> Failure(string error) => new(false, default, error);
    public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(string error) => Failure(error);
}
