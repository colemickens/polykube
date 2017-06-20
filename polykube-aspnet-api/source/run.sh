#!/usr/bin/env bash

# workaround
killall -9 dotnet

dotnet run \
	--project aspnet-api/aspnet-api.csproj


# workaround
killall -9 dotnet
