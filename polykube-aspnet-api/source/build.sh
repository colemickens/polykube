#!/usr/bin/env bash

set -xeu

dotnet restore aspnet-api/aspnet-api.csproj
ls
pwd
dotnet -v publish -c Release ./aspnet-api/aspnet-api.csproj
mkdir -p ../dist
cp -a aspnet-api/bin/Release/netcoreapp1.0/* ../dist/
