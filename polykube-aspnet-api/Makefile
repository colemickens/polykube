.NOTPARALLEL:
.PHONY: devenv dist image instance push

CURDIR = $(shell pwd)
REGISTRY ?= 127.0.0.1:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-aspnet-api
IMAGE_DEVENV = $(PROJECT)-devenv
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

devenv:
	docker build -t $(IMAGE_DEVENV) ./source
	docker run -it --net=host \
		--env=LISTEN_PORT=9080 \
		--env=REDIS_HOST=127.0.0.1 \
		--env=REDIS_PORT=6379 \
		--env=REDIS_PASSWORD=redis \
		--env=POSTGRES_HOST=127.0.0.1 \
		--env=POSTGRES_PORT=5432 \
		--env=POSTGRES_DATABASE_NAME=polykubedb \
		--env=POSTGRES_USERNAME=postgres \
		--env=POSTGRES_PASSWORD=postgres \
		-v $(CURDIR)/source:/$(PROJECT)/source \
		-v $(CURDIR)/.nuget-cache:/root/.local/share/NuGet \
			$(IMAGE_DEVENV) /bin/bash || true

dist:
	docker build -t $(IMAGE_DEVENV) ./source
	docker rm -f $(PROJECT) || true
	docker run --name=$(PROJECT) $(IMAGE_DEVENV) bash ./build.sh
	rm -rf image/dist
	docker cp $(PROJECT):/$(PROJECT)/dist image/

image: dist
	cd image && \
	docker build -t $(IMAGE_FINAL) .

instance: image
	docker run -it --net=host \
		--env=LISTEN_PORT=9080 \
		--env=REDIS_HOST=127.0.0.1 \
		--env=REDIS_PORT=6379 \
		--env=REDIS_PASSWORD=redis \
		--env=POSTGRES_HOST=127.0.0.1 \
		--env=POSTGRES_PORT=5432 \
		--env=POSTGRES_DATABASE_NAME=polykubedb \
		--env=POSTGRES_USERNAME=postgres \
		--env=POSTGRES_PASSWORD=postgres \
			$(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)
