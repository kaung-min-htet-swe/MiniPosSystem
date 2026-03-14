namespace Common;

public class Result<TValue>
{
    public bool IsSuccess { get; }
    public TValue? Data { get; }
    public Error? Error { get; }

    public Result(bool isSuccess, TValue? data, Error? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }
    
    public static Result<TValue> Success(TValue data) => new Result<TValue>(true, data, null);
    public static Result<TValue> Failure(Error error) => new Result<TValue>(false, default, error);
}