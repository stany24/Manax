using ManaxLibrary.DTO.Rank;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiRankClient
{
    public static async Task<Optional<List<RankDto>>> GetRanksAsync()
    {
        return await ManaxApiClient.GetAsync<List<RankDto>>("api/ranks");
    }

    public static async Task<Optional<bool>> CreateRankAsync(RankCreateDto rank)
    {
        return await ManaxApiClient.PostSuccessAsync("api/rank", rank);
    }

    public static async Task<Optional<bool>> UpdateRankAsync(RankUpdateDto rank)
    {
        return await ManaxApiClient.PutSuccessAsync("api/rank", rank);
    }

    public static async Task<Optional<bool>> DeleteRankAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/rank/{id}");
    }

    public static async Task<Optional<bool>> SetUserRankAsync(UserRankCreateDto rank)
    {
        return await ManaxApiClient.PostSuccessAsync("api/rank/set", rank);
    }

    public static async Task<Optional<List<UserRankDto>>> GetRankingAsync()
    {
        return await ManaxApiClient.GetAsync<List<UserRankDto>>("api/ranking");
    }
}