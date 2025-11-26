using ManaxLibrary.DTO.Person;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiPersonClient
{
    public static async Task<Optional<List<PersonDto>>> GetPersonsAsync()
    {
        return await ManaxApiClient.GetAsync<List<PersonDto>>("api/persons");
    }

    public static async Task<Optional<bool>> CreatePersonAsync(PersonCreateDto person)
    {
        return await ManaxApiClient.PostSuccessAsync("api/person", person);
    }

    public static async Task<Optional<bool>> UpdatePersonAsync(long id, PersonUpdateDto person)
    {
        return await ManaxApiClient.PutSuccessAsync($"api/person/{id}", person);
    }

    public static async Task<Optional<bool>> DeletePersonAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/person/{id}");
    }
}