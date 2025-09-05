# mekatrol-cam
A set of CNC/CAM utilities built into a cross platform desktop.

## Formatting code
```bash
dotnet format --verbosity diagnostic
```

## Vuild

```bash
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r osx-x64 --self-contained true
dotnet publish -c Release -r linux-x64 --self-contained true
dotnet publish -c Release -r linux--arm64 --self-contained true
```
