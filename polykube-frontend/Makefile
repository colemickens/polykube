.NOTPARALLEL:
.PHONY: devenv dist image instance push

CURDIR = $(shell pwd)
REGISTRY ?= localhost:5000
VERSION ?= $(shell git describe --always --dirty)

PROJECT = polykube-frontend
IMAGE_DEVENV = $(PROJECT)-devenv
IMAGE_DIST = $(PROJECT)-dist
IMAGE_FINAL = $(REGISTRY)/$(PROJECT):$(VERSION)

devenv:
	docker build -t $(IMAGE_DEVENV) ./source
	docker run -it --net=host \
		-v $(CURDIR)/source:/$(PROJECT)/source \
		-v $(CURDIR)/.node_modules-cache:/$(PROJECT)/source/polykube-frontend/node_modules \
			$(IMAGE_DEVENV) bash -c "./build.sh && ./run.sh; bash" \
	|| true

dist:
	rm -rf image/dist
	docker rm -f $(PROJECT) || true
	docker build --build-arg=BUILD=y -t $(IMAGE_DIST) ./source
	docker run --name $(PROJECT) $(IMAGE_DEVENV)
	docker cp $(PROJECT):/$(PROJECT)/dist image/

image: dist
	cd image && \
	docker build -t $(IMAGE_FINAL) .

instance: image
	docker run -it --net=host $(IMAGE_FINAL)

push: image
	docker push $(IMAGE_FINAL)
