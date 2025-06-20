rm -r Migrations
dotnet ef migrations add UserV0.1 --project ManaxApi.csproj --context UserContext
dotnet ef migrations add LibraryV0.1 --project ManaxApi.csproj --context LibraryContext
dotnet ef migrations add SerieV0.1 --project ManaxApi.csproj --context SerieContext
dotnet ef migrations add ChapterV0.1 --project ManaxApi.csproj --context ChapterContext
rm -r bin
