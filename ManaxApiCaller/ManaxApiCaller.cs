namespace ManaxApiCaller;

public static class ManaxApiCaller
{
    internal static readonly HttpClient Client = new() { BaseAddress = new Uri("http://localhost:5000/") };

    public static void setHost(Uri host)
    {
        Client.BaseAddress = host;
    }
}