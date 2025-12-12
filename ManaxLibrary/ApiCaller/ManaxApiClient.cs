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
    
    internal static HttpClient UploadClient = new()
    {
        BaseAddress = new Uri("http://127.0.0.1:5246/"),
        Timeout = TimeSpan.FromSeconds(60)
    };

    public static void SetHost(Uri host)
    {
        HttpClientHandler handler = new();
        Client = new HttpClient(handler)
        {
            BaseAddress = host,
            Timeout = TimeSpan.FromSeconds(5)
        };
        UploadClient = new HttpClient(handler)
        {
            BaseAddress = host,
            Timeout = TimeSpan.FromSeconds(60)
        };
    }

    public static void SetToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        UploadClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (Client.BaseAddress == null) return;
        _ = NotificationReceiver.InitializeAsync(Client.BaseAddress, token);
    }

    public static void ResetToken()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        UploadClient.DefaultRequestHeaders.Authorization = null;
    }

    internal static async Task<Optional<T>> ExecuteWithErrorHandlingAsync<T>(Func<Task<Optional<T>>> apiCall)
    {
        try
        {
            return await apiCall();
        }
        catch (TaskCanceledException)
        {
            return Optional<T>.Failure("Timeout");
        }
        catch (Exception e)
        {
            return Optional<T>.Failure("Exception: " + e.Message);
        }
    }

    internal static async Task<Optional<T>> GetAsync<T>(string endpoint, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return Optional<T>.Failure(response);
            T? data = await response.Content.ReadFromJsonAsync<T>();
            return data == null
                ? Optional<T>.Failure(errorMessage ?? $"Failed to deserialize {typeof(T).Name} from API response.")
                : Optional<T>.Success(data);
        });
    }

    internal static async Task<Optional<byte[]>> GetBytesAsync(string endpoint, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return Optional<byte[]>.Failure(response);
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            return data.Length == 0
                ? Optional<byte[]>.Failure(errorMessage ?? $"Empty data received from {endpoint}.")
                : Optional<byte[]>.Success(data);
        });
    }

    internal static async Task<Optional<TResult>> PostAsync<TResult, TBody>(string endpoint, TBody body, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, body);
            if (!response.IsSuccessStatusCode) return Optional<TResult>.Failure(response);
            TResult? data = await response.Content.ReadFromJsonAsync<TResult>();
            return data == null
                ? Optional<TResult>.Failure(errorMessage ?? $"Failed to deserialize {typeof(TResult).Name} from API response.")
                : Optional<TResult>.Success(data);
        });
    }

    internal static async Task<Optional<bool>> PostSuccessAsync<TBody>(string endpoint, TBody body)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, body);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
        });
    }

    internal static async Task<Optional<bool>> PostSuccessAsync(string endpoint)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PostAsync(endpoint, null);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
        });
    }

    internal static async Task<Optional<TResult>> PutAsync<TResult, TBody>(string endpoint, TBody body, string? errorMessage = null)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PutAsJsonAsync(endpoint, body);
            if (!response.IsSuccessStatusCode) return Optional<TResult>.Failure(response);
            TResult? data = await response.Content.ReadFromJsonAsync<TResult>();
            return data == null
                ? Optional<TResult>.Failure(errorMessage ?? $"Failed to deserialize {typeof(TResult).Name} from API response.")
                : Optional<TResult>.Success(data);
        });
    }

    internal static async Task<Optional<bool>> PutSuccessAsync<TBody>(string endpoint, TBody body)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PutAsJsonAsync(endpoint, body);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
        });
    }

    internal static async Task<Optional<bool>> PutSuccessAsync(string endpoint)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.PutAsync(endpoint, null);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
        });
    }

    internal static async Task<Optional<bool>> DeleteAsync(string endpoint)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await Client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
        });
    }
}