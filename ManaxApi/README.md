# Creating first migration for table
```bash
dotnet ef migrations add <migrationName> --project ManaxApi/ManaxApi.csproj --context <ContextName>
```

# Merge migrations after x
delete all migrations after x and then run
```bash
dotnet ef migrations add SquashedAfterRelease2025 --project ManaxApi/ManaxApi.csproj
```