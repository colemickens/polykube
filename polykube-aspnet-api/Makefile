.NOTPARALLEL:

CURDIR = $(shell pwd)
REGISTRY ?= localhost:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-aspnet-api
IMAGE_DEVENV = $(PROJECT)-devenv
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

build:
	dotnet restore
	dotnet -v publish -c Release aspnet-api/project.json

run:
	dotnet run aspnet-api/project.json

_devenv-build:
	cd _devenv; docker build --pull -t $(IMAGE_DEVENV) .

devenv: _devenv-build
	docker run -it -p 0.0.0.0:8000:8000 \
		-v $(CURDIR):/$(PROJECT) \
		-v $(CURDIR)/.nuget-cache:/root/.local/share/NuGet \
			$(IMAGE_DEVENV) /bin/bash || true

# TODO: consider:
# rather than mounting, we add the source in the Dockerfile
# it gets overriden in devenv, but we get pristine build container otherwise
# TODO: this also makes cached builds no-ops potentially... which would be nice to have back
dbuild: _devenv-build
	docker run -v $(CURDIR):/$(PROJECT) $(IMAGE_DEVENV) make build

image: dbuild
	(\
		cd _buildenv; mkdir -p dist \
		&& cp -a ../aspnet-api/bin/Release/netcoreapp1.0/debian.8-x64/* ./dist/ \
		&& docker build -t $(IMAGE_FINAL) . \
	)

instance: image
	docker run -it -p 0.0.0.0:9999:80 $(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)
