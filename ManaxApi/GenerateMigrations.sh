rm -r Migrations
dotnet ef migrations add UserV0.1 --project ManaxApi.csproj --context UserContext
dotnet ef migrations add LibraryV0.1 --project ManaxApi.csproj --context LibraryContext
dotnet ef migrations add SerieV0.1 --project ManaxApi.csproj --context SerieContext
dotnet ef migrations add ChapterV0.1 --project ManaxApi.csproj --context ChapterContext
dotnet ef migrations add IssueV0.1 --project ManaxApi.csproj --context IssueContext
dotnet ef migrations add ReadV0.1 --project ManaxApi.csproj --context ReadContext
rm -r bin
