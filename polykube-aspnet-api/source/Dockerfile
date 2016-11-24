FROM docker.io/microsoft/dotnet:1.1.0-sdk-msbuild
#FROM docker.io/microsoft/dotnet:1.1.0-sdk-projectjson

RUN apt-get update -qq && apt-get install -qqy make \
	&& rm -rf /var/lib/apt/lists/*

ADD . /polykube-aspnet-api/source/

WORKDIR /polykube-aspnet-api/source
CMD ./build.sh
