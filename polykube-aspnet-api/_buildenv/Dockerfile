FROM docker.io/microsoft/dotnet:1.0.1-core-deps

ADD dist/ /polykube-aspnet-api/

WORKDIR /polykube-aspnet-api/
CMD ./publish/aspnet-api
