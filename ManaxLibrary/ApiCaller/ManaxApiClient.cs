using System.Net.Http.Headers;
using System.Net.Http.Json;
using ManaxLibrary.Notifications;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiClient
{
    internal static HttpClient Client = new()
    {
        BaseAddress = new Uri("http://127.0.0.1:5246/"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    public static void SetHost(Uri host)
    {
        HttpClientHandler handler = new();
        Client = new HttpClient(handler)
        {
            BaseAddress = host,
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public static void SetToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (Client.BaseAddress == null) return;
        _ = NotificationReceiver.InitializeAsync(Client.BaseAddress, token);
    }

    public static void ResetToken()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    internal static async Task<Optional<T>> ExecuteWithErrorHandlingAsync<T>(Func<Task<Optional<T>>> apiCall)
    {
        try
        {
            return await apiCall();
        }
        catch (TaskCanceledException)
        {
            return new Optional<T>("Timeout");
        }
        catch (Exception e)
        {
            return new Optional<T>("Exception: " + e.Message);
        }
    }

    // Generic GET method that returns deserialized JSON data
    internal static async Task<Optional<T>> GetAsync<T>(string endpoint, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return new Optional<T>(response);
            T? data = await response.Content.ReadFromJsonAsync<T>();
            return data == null
                ? new Optional<T>(errorMessage ?? $"Failed to deserialize {typeof(T).Name} from API response.")
                : new Optional<T>(data);
        });
    }

    // Generic GET method for binary data (images, zips, etc.)
    internal static async Task<Optional<byte[]>> GetBytesAsync(string endpoint, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return new Optional<byte[]>(response);
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            return data.Length == 0
                ? new Optional<byte[]>(errorMessage ?? $"Empty data received from {endpoint}.")
                : new Optional<byte[]>(data);
        });
    }

    // Generic POST method that returns deserialized JSON data
    internal static async Task<Optional<TResult>> PostAsync<TResult, TBody>(string endpoint, TBody body, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, body);
            if (!response.IsSuccessStatusCode) return new Optional<TResult>(response);
            TResult? data = await response.Content.ReadFromJsonAsync<TResult>();
            return data == null
                ? new Optional<TResult>(errorMessage ?? $"Failed to deserialize {typeof(TResult).Name} from API response.")
                : new Optional<TResult>(data);
        });
    }

    // Generic POST method that returns bool (success indicator)
    internal static async Task<Optional<bool>> PostSuccessAsync<TBody>(string endpoint, TBody body)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, body);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    // POST method with null body that returns bool
    internal static async Task<Optional<bool>> PostSuccessAsync(string endpoint)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PostAsync(endpoint, null);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    // Generic PUT method that returns deserialized JSON data
    internal static async Task<Optional<TResult>> PutAsync<TResult, TBody>(string endpoint, TBody body, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PutAsJsonAsync(endpoint, body);
            if (!response.IsSuccessStatusCode) return new Optional<TResult>(response);
            TResult? data = await response.Content.ReadFromJsonAsync<TResult>();
            return data == null
                ? new Optional<TResult>(errorMessage ?? $"Failed to deserialize {typeof(TResult).Name} from API response.")
                : new Optional<TResult>(data);
        });
    }

    // Generic PUT method that returns bool (success indicator)
    internal static async Task<Optional<bool>> PutSuccessAsync<TBody>(string endpoint, TBody body)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PutAsJsonAsync(endpoint, body);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    // PUT method with null body that returns bool
    internal static async Task<Optional<bool>> PutSuccessAsync(string endpoint)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PutAsync(endpoint, null);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    // Generic DELETE method that returns bool (success indicator)
    internal static async Task<Optional<bool>> DeleteAsync(string endpoint)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}