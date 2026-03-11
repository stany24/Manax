namespace ManaxLibrary;

public class Optional<TReturn>
{
    private readonly TReturn? _value;

    private Optional(TReturn value)
    {
        _value = value;
    }

    private Optional(string error)
    {
        Error = error;
    }

    public string Error { get; } = string.Empty;
    public bool Failed => Error != string.Empty;
    public bool Succeeded => !Failed;

    public static Optional<TReturn> Success(TReturn value)
    {
        return new Optional<TReturn>(value);
    }

    public static Optional<TReturn> Failure(string error)
    {
        return new Optional<TReturn>(error);
    }
    
    public static Optional<TReturn> Failure(int errorCode)
    {
        return new Optional<TReturn>(errorCode.ToString());
    }

    public static Optional<TReturn> Failure(HttpResponseMessage response)
    {
        string error = response.StatusCode + ": " + response.Content.ReadAsStringAsync().Result;
        return new Optional<TReturn>(error);
    }

    public TReturn GetValue()
    {
        return Failed ? throw new InvalidOperationException("Cannot get value from an Optional that failed.") : _value!;
    }
}