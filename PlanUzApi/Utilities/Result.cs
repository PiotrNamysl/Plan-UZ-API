namespace PlanUzApi.Utilities;

public enum ResultStatus
{
    Success = 0,
    Warning,
    Error
}

public enum ResultStatusCode
{
    Ok = 0,
    NoDataFound,
    DataAlreadyExist,
    AccessForbidden,
    BadRequest,
    UnknownError,
    InternalServerError
}

public interface IResult
{
    public ResultStatus Status { get; init; }
    public ResultStatusCode StatusCode { get; init; }
    public string Message { get; init; }

    public bool IsSuccess => Status == ResultStatus.Success;
    public bool IsNotSuccess => Status != ResultStatus.Success;
    public bool IsWarning() => Status == ResultStatus.Warning;
    public bool IsError() => Status != ResultStatus.Error;
}

public interface IResult<T> : IResult where T : class
{
    public T Data { get; init; }
}

public class Result(ResultStatus status, ResultStatusCode statusCode, string message) : IResult
{
    public ResultStatus Status { get; init; } = status;
    public ResultStatusCode StatusCode { get; init; } = statusCode;
    public string Message { get; init; } = message;

    public static Result Success() => new Result(ResultStatus.Success, ResultStatusCode.Ok, string.Empty);
    public static Result Success(ResultStatusCode statusCode) => new(ResultStatus.Success, statusCode, string.Empty);
    public static Result Success(ResultStatusCode statusCode, string message) => new(ResultStatus.Success, statusCode, message);

    public static Result Warning(ResultStatusCode statusCode) => new(ResultStatus.Warning, statusCode, string.Empty);
    public static Result Warning(ResultStatusCode statusCode, string message) => new(ResultStatus.Warning, statusCode, message);

    public static Result Error(ResultStatusCode statusCode) => new(ResultStatus.Error, statusCode, string.Empty);
    public static Result Error(ResultStatusCode statusCode, string message) => new(ResultStatus.Error, statusCode, message);
}

public class Result<T>(T data, ResultStatus status, ResultStatusCode statusCode, string message, DateTime? lastUpdate = null)
    : IResult<T> where T : class
{
    public T Data { get; init; } = data;
    public ResultStatus Status { get; init; } = status;
    public ResultStatusCode StatusCode { get; init; } = statusCode;
    public string Message { get; init; } = message;
    public DateTime? LastUpdate { get; set; } = lastUpdate;

    public static Result<T> Success(T data) => new(data, ResultStatus.Success, ResultStatusCode.Ok, string.Empty, null);
    public static Result<T> Success(ResultStatusCode statusCode) => new(null, ResultStatus.Success, statusCode, string.Empty, null);
    public static Result<T> Success(ResultStatusCode statusCode, string message) => new(null, ResultStatus.Success, statusCode, message, null);
    public static Result<T> Success(T data, ResultStatusCode statusCode) => new(data, ResultStatus.Success, statusCode, string.Empty, null);
    public static Result<T> Success(T data, DateTime lastUpdate) => new(data, ResultStatus.Success, ResultStatusCode.Ok, string.Empty, lastUpdate);
    public static Result<T> Success(T data, ResultStatusCode statusCode, string message) => new(data, ResultStatus.Success, statusCode, message, null);

    public static Result<T> Warning(ResultStatusCode statusCode) => new(null, ResultStatus.Warning, statusCode, string.Empty, null);
    public static Result<T> Warning(ResultStatusCode statusCode, string message) => new(null, ResultStatus.Warning, statusCode, message, null);
    public static Result<T> Warning(T data, ResultStatusCode statusCode) => new(data, ResultStatus.Warning, statusCode, string.Empty, null);
    public static Result<T> Warning(T data, ResultStatusCode statusCode, string message) => new(data, ResultStatus.Warning, statusCode, message, null);

    public static Result<T> Warning(T data, ResultStatusCode statusCode, DateTime lastUpdate) => new(data, ResultStatus.Warning, statusCode, string.Empty, lastUpdate);
    public static Result<T> Error(ResultStatusCode statusCode) => new(null, ResultStatus.Error, statusCode, string.Empty, null);
    public static Result<T> Error(ResultStatusCode statusCode, string message) => new(null, ResultStatus.Error, statusCode, message, null);
    public static Result<T> Error(T data, ResultStatusCode statusCode) => new(data, ResultStatus.Error, statusCode, string.Empty, null);
    public static Result<T> Error(T data, ResultStatusCode statusCode, string message) => new(data, ResultStatus.Error, statusCode, message, null);
    public static Result<T> Error(T data, ResultStatusCode statusCode, DateTime lastUpdate) => new(data, ResultStatus.Error, statusCode, string.Empty, lastUpdate);
}

public static class ResultStatusCodeExtension
{
    public static bool IsOkStatusCode(this IResult result) => result.StatusCode == ResultStatusCode.Ok;
    public static bool IsNotOkStatusCode(this IResult result) => result.StatusCode != ResultStatusCode.Ok;
}