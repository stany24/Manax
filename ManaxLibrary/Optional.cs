namespace ManaxLibrary;

public class Optional<TReturn>
{
    public string Error { get; } = string.Empty;
    public bool Failed => Error != string.Empty;
    public bool Succeeded => !Failed;
    private readonly TReturn? _value;

    private Optional(TReturn value)
    {
        _value = value;
    }
    
    private Optional(string error)
    {
        Error = error;
    }
    
    public static Optional<TReturn> Success(TReturn value) => new(value);
    
    public static Optional<TReturn> Failure(string error) => new(error);
    
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