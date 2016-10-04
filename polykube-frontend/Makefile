.NOTPARALLEL:

CURDIR = $(shell pwd)
REGISTRY ?= localhost:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-frontend
IMAGE_DEVENV = $(PROJECT)-devenv
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

prep:
	npm install

build: prep
	ng build -prod

serve:
	ng serve --host 0.0.0.0 --port 9999 --live-reload-host 0.0.0.0 --live-reload-port 9998;

_devenv-build:
	cd _devenv; docker build --pull -t $(IMAGE_DEVENV) .

devenv: _devenv-build
	docker run -it -p 0.0.0.0:9999:9999 -p 0.0.0.0:9998:9998 -v $(CURDIR):/$(PROJECT) $(IMAGE_DEVENV) /bin/bash || true

dbuild: _devenv-build
	docker run -v $(CURDIR):/$(PROJECT) $(IMAGE_DEVENV) make build

image: dbuild
	(\
		cd _buildenv; mkdir -p dist \
		&& cp -a ../dist/* ./dist/ \
		&& docker build -t $(IMAGE_FINAL) . \
	)

instance: image
	docker run -it -p 0.0.0.0:9999:80 $(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)
