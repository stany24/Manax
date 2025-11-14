using System.Net.Http.Json;
using ManaxLibrary.DTO.Person;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiPersonClient
{
    public static async Task<Optional<List<PersonDto>>> GetPersonsAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/persons");
            if (!response.IsSuccessStatusCode) return new Optional<List<PersonDto>>(response);
            List<PersonDto>? persons = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
            return persons == null
                ? new Optional<List<PersonDto>>("Failed to read persons from response.")
                : new Optional<List<PersonDto>>(persons);
        });
    }

    public static async Task<Optional<bool>> CreatePersonAsync(PersonCreateDto person)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/person", person);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> UpdatePersonAsync(long id, PersonUpdateDto person)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/person/{id}", person);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> DeletePersonAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/person/{id}");
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}