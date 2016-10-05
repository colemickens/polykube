.NOTPARALLEL:

VERSION ?= $(shell git describe --always --dirty)
NAMESPACE ?= polykube
REDIS_PASSWORD ?= password
POSTGRES_PASSWORD ?= password

ifndef REGISTRY
    $(error REGISTRY is required to be set)
endif

_:
	${info Must specify a command.}

deploy-all: | deploy-registry deploy-monitoring deploy-logging deploy-kube-lego deploy-polykube

deploy-polykube:
	$(eval TEMPDIR := $(shell mktemp -d))
	cp *.yaml $(TEMPDIR)
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{namespace}}|$(NAMESPACE)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{repo}}|$(REGISTRY)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{postgres_password}}|$(POSTGRES_PASSWORD)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{redis_password}}|$(REDIS_PASSWORD)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{version}}|$(VERSION)|g"
	kubectl apply -f $(TEMPDIR)
	rm -rf $(TEMPDIR)

deploy-registry:
	find addons/kube-registry/ -type f -name "*.yaml" -exec kubectl apply -f {} \;
deploy-monitoring:
	find addons/kube-monitoring/ -type f -name "*.yaml" -exec kubectl apply -f {} \;
deploy-logging:
	find addons/kube-logging/ -type f -name "*.yaml" -exec kubectl apply -f {} \;
deploy-kube-lego:
	find addons/kube-lego/ -type f -name "*.yaml" -exec kubectl apply -f {} \;
	export kube_lego_ip=""; \
	while : ; do \
		kube_lego_ip=$$(kubectl get svc nginx --namespace=nginx-ingress --template="{{range .status.loadBalancer.ingress}}{{.ip}}{{end}}"); \
		[ -z "$${kube_lego_ip}" ] || break; sleep 5; \
	done

## TODO: probably remove the following:

magic-polykube: | registry-start deploy-polykube registry-stop

registry-start:
	$(eval REGISTRY_POD_NAME = $(shell kubectl get pods --selector=k8s-app=kube-registry --namespace=kube-system --output=jsonpath="{.items[0].metadata.name}"))
	kubectl port-forward --namespace=kube-system ${REGISTRY_POD_NAME} 5000 & >/dev/null

registry-stop:
	$(eval PID = $(shell pgrep -a kubectl | grep "port-forward --namespace=kube-system kube-registry-" | grep "5000" | cut -d' ' -f1))
	kill $(PID)

