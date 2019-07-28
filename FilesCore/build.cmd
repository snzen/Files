dotnet publish -c release -r win-x64 --self-contained false
dotnet publish -c release -r win-arm64 --self-contained false
dotnet publish -c release -r linux-x64 --self-contained false
dotnet publish -c release -r linux-arm --self-contained false