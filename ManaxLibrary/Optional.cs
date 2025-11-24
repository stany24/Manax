using ManaxLibrary.Logging;

namespace ManaxLibrary;

public class Optional<TReturn>
{
    public string Error { get; } = string.Empty;
    public bool Failed => Error != string.Empty;
    private readonly TReturn? _value;

    public Optional(TReturn value)
    {
        _value = value;
    }
    
    public Optional(string error, bool isError = true)
    {
        if (isError)
        {
            Error = error;
            Logger.LogFailure(error);
        }
        else
        {
            if (typeof(TReturn) != typeof(string))
                throw new InvalidOperationException("Optional must be of type string when isError is false.");
            _value = (TReturn)(object)error;
        }
    }

    public Optional(HttpResponseMessage response)
    {
        string error = response.StatusCode + ": " + response.Content.ReadAsStringAsync().Result;
        Error = error;
        Logger.LogFailure(error);
    }

    public TReturn GetValue()
    {
        return Failed ? throw new InvalidOperationException("Cannot get value from an Optional that failed.") : _value!;
    }
}