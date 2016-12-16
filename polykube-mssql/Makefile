.NOTPARALLEL:
.PHONY: image instance push

CURDIR = $(shell pwd)
REGISTRY ?= localhost:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-mssql
IMAGE_DEVENV = $(IMAGE_FINAL)
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

image:
	cd image && \
	docker build -t $(IMAGE_FINAL) .

instance: image
	docker run -it --net=host \
		--env=SA_PASSWORD='SequelP@ssw0rd' \
		--env=ACCEPT_EULA=Y \
			$(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)

