.NOTPARALLEL:

all:
	${info Specify a command: (doit, no-cache-ify)}

magic: _magic-prep
	$(MAKE) -C ./polykube-aspnet-api-nginx push
	$(MAKE) -C ./polykube-aspnet-api push
	$(MAKE) -C ./polykube-frontend push
	$(MAKE) -C ./polykube-postgres push
	$(MAKE) -C ./polykube-redis push
	$(MAKE) -C ./deployment deploy-polykube

magic-proxy:
	$(MAKE) -C ./deployment registry-start
	$(MAKE) -C ./polykube-aspnet-api-nginx push
	$(MAKE) -C ./polykube-aspnet-api push
	$(MAKE) -C ./polykube-frontend push
	$(MAKE) -C ./polykube-postgres push
	$(MAKE) -C ./polykube-redis push
	$(MAKE) -C ./deployment registry-stop
	$(MAKE) -C ./deployment deploy-polykube

_magic-prep:
ifndef REGISTRY
    $(error REGISTRY is undefined. Use 'magic-proxy' if you want to use a cluster-local registry.)
endif

no-cache-ify:
	# don't do this if git is dirty, because we don't want to mess up the workspace
	git diff-index --quiet HEAD 
	find . -type f -name 'Makefile' -exec sed -i '' s/make build/make build --no-cache/ {} +
