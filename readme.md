# mekatrol-cam
A set of CNC/CAM utilities built into a cross platform desktop.

## Formatting code
```bash
dotnet format --verbosity diagnostic
```

## Build

```bash
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r win-x64 --self-contained true -o ./publish/win-x64
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r osx-x64 --self-contained true -o ./publish/osx-x64
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r linux-arm64 --self-contained true -o ./publish/linux-arm64
```

## Vulnerability/deprecated package checks

```bash
`dotnet list package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json`
`dotnet list package --deprecated --include-transitive --source https://api.nuget.org/v3/index.json`
`dotnet list package --outdated   --include-transitive --source https://api.nuget.org/v3/index.json`
```