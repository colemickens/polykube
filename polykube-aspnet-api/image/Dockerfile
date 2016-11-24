FROM docker.io/microsoft/dotnet:1.1.0-runtime

ADD dist/ /polykube-aspnet-api/dist

WORKDIR /polykube-aspnet-api/dist

CMD dotnet ./publish/aspnet-api.dll
