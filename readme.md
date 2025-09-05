# mekatrol-cam
A set of CNC/CAM utilities built into a cross platform desktop.

## Formatting code
```bash
dotnet format --verbosity diagnostic
```

## Vuild

```bash
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r win-x64 --self-contained true -o ./publish/win-x64
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r osx-x64 --self-contained true -o ./publish/osx-x64
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64
dotnet publish MekatrolCAM/MekatrolCAM.csproj -c Release -r linux-arm64 --self-contained true -o ./publish/linux-arm64
```
