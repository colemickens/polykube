.NOTPARALLEL:
.PHONY: image instance push

CURDIR = $(shell pwd)
REGISTRY ?= localhost:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-aspnet-api-nginx
IMAGE_DEVENV = $(IMAGE_FINAL)
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

image:
	cd image && \
	docker build -t $(IMAGE_FINAL) .

instance: image
	docker run -it --net=host \
		--env=BACKEND_HOST=127.0.0.1 \
		--env=BACKEND_PORT=9080 \
		--env=LISTEN_PORT=9000 \
			$(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)
