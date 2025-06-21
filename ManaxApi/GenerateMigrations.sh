rm -r Migrations
dotnet ef migrations add ManaxV0.1 --project ManaxApi.csproj --context ManaxContext
rm -r bin
