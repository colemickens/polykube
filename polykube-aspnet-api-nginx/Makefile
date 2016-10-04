.NOTPARALLEL:

CURDIR = $(shell pwd)
REGISTRY ?= localhost:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-aspnet-api-nginx
IMAGE_DEVENV = $(IMAGE_FINAL)
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

devenv: image
	docker run -it -p 80:80 -v $(CURDIR):/$(PROJECT) $(IMAGE_DEVENV) /bin/bash || true

image:
	cd _buildenv; docker build -t $(IMAGE_FINAL) .

instance: image
	docker run -it -p 80:80 $(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)
