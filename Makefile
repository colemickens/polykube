_:
	${info Specify a command: (doit, no-cache-ify)}

ifndef REGISTRY
    $(error REGISTRY is undefined. Use 'magic-proxy' if you want to use a cluster-local registry.)
endif

magic: | _magic-prep push-all deploy

push-all:
	$(MAKE) -C ./polykube-aspnet-api-nginx push
	$(MAKE) -C ./polykube-aspnet-api push
	$(MAKE) -C ./polykube-frontend push
	$(MAKE) -C ./polykube-postgres push
	$(MAKE) -C ./polykube-redis push

deploy:
	$(MAKE) -C ./deployment

_magic-prep:

no-cache-ify:
	# don't do this if git is dirty, because we don't want to mess up the workspace
	git diff-index --quiet HEAD 
	find . -type f -name 'Makefile' -exec sed -i '' s/make build/make build --no-cache/ {} +
