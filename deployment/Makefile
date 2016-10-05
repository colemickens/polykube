TEMPDIR = $(shell mktemp -d)
VERSION ?= $(shell git describe --always --dirty)
NAMESPACE ?= polykube
REDIS_PASSWORD ?= password
POSTGRES_PASSWORD ?= password

ifndef REGISTRY
    $(error REGISTRY is required to be set)
endif

_:
	${info Must specify a command.}

deploy-all: | deploy-registry deploy-monitoring deploy-logging deploy-kube-lego wait-kube-lego deploy-polykube

deploy-polykube:
	$(eval TEMPDIR = $(shell mktemp -d))
	cp *.yaml $(TEMPDIR)/
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{namespace}}|$(NAMESPACE)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{repo}}|$(REGISTRY)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{postgres_password}}|$(POSTGRES_PASSWORD)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{redis_password}}|$(REDIS_PASSWORD)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{version}}|$(VERSION)|g"
	kubectl apply -f $(TEMPDIR)
	$(shell rm -rf $(TEMPDIR))

deploy-registry:
	kubectl apply -f addons/kube-registry/
deploy-monitoring:
	kubectl apply -f addons/kube-monitoring/
deploy-logging:
	kubectl apply -f addons/kube-logging/
deploy-kube-lego:
	kubectl apply -f addons/kube-lego/
wait-kube-lego:
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

