namespace MyBudg.Common;

public class Result
{
    public bool Succeeded => Error is null;
    public string? Error { get; protected init; }

    public static Result Ok() => new();
    public static Result Fail(string error) => new() { Error = error };
}

public class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Ok(T value) => new() { Value = value };
    public new static Result<T> Fail(string error) => new() { Error = error };
}
