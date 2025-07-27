namespace HotelManagement.Application.Common.Models;

public class Result<T>
{
    private Result(bool succeeded, int statusCode, T? data, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
        Data = data;
        Errors = errors?.ToList() ?? [];
    }

    public bool Succeeded { get; init; }
    public int StatusCode { get; init; }
    public T? Data { get; init; }
    public List<string> Errors { get; init; }

    public static Result<T> Success(T data, int statusCode = 200)
    {
        return new Result<T>(true, statusCode, data);
    }

    public static Result<T> Failure(IEnumerable<string> errors, int statusCode = 400)
    {
        return new Result<T>(false, statusCode, default, errors);
    }

    public static Result<T> Failure(string error, int statusCode = 400)
    {
        return new Result<T>(false, statusCode, default, [error]);
    }
}
