namespace ManaxLibrary.ApiCaller;

public static class ManaxApiConfig
{
    public static void SetHost(Uri host)
    {
        ManaxApiClient.SetHost(host);
    }

    public static void SetToken(string token)
    {
        ManaxApiClient.SetToken(token);
    }
}