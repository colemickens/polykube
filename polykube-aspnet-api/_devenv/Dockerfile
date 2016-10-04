FROM docker.io/microsoft/dotnet:1.0.0-preview2-sdk

RUN apt-get update -qq && apt-get install -qqy make \
	&& rm -rf /var/lib/apt/lists/*

WORKDIR /polykube-aspnet-api
CMD /bin/bash
