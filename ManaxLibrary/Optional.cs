using ManaxLibrary.Logging;

namespace ManaxLibrary;

public class Optional<T>
{
    public string Error { get; } = string.Empty;
    private readonly T? _value;
    public bool Failed => Error != string.Empty;

    public Optional(T value)
    {
        _value = value;
    }

    /// <summary>
    /// When T is a string set isError to false to set the value instead of the error.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="isError"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public Optional(string error, bool isError = true)
    {
        if (isError)
        {
            Error = error;
            Logger.LogFailure(error, Environment.StackTrace);
        }
        else
        {
            
            if (typeof(T) != typeof(string))
                throw new InvalidOperationException("Optional must be of type string when isError is false.");
            _value = (T)(object)error;
        }
    }



    public Optional(HttpResponseMessage response)
    {
        string error = response.StatusCode + ": " + response.ReasonPhrase;
        Error = error;
        Logger.LogFailure(error, Environment.StackTrace);
    }
    
    /// <summary>
    /// Return the value if the Optional is successful, you should call this method in a code path where Failed == false.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T GetValue()
    {
        if (Failed)
            throw new InvalidOperationException("Cannot get value from an Optional that failed.");
        return _value!;
    }
}