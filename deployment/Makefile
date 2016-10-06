.NOTPARALLEL:

KUBECTL := $(shell command -v kubectl 2> /dev/null)

VERSION ?= $(shell git describe --always --dirty)
NAMESPACE ?= polykube

ifndef REGISTRY
    $(error REGISTRY is required to be set)
endif
ifndef KUBECTL
    $(error kubectl must be availabe on the PATH)
endif

_:
	${info Must specify a command.}

# NOTE: kube-registry is not included by default
deploy-all: | deploy-kube-lego deploy-logging deploy-monitoring deploy-polykube wait-kube-lego set-cf-ip

deploy-polykube:
	$(eval TEMPDIR := $(shell mktemp -d))
	$(eval REDIS_PASSWORD ?= $(shell echo secretpassword | base64 -w 0))
	$(eval POSTGRES_PASSWORD ?= $(shell echo secretpassword | base64 -w 0))
	$(eval REDIS_PASSWORD_BASE64 := $(shell echo $(REDIS_PASSWORD) | base64 -w 0))
	$(eval POSTGRES_PASSWORD_BASE64 := $(shell echo $(POSTGRES_PASSWORD) | base64 -w 0))
	echo $(POSTGRES_PASSWORD)
	echo $(REDIS_PASSWORD)
	cp polykube/*.yaml $(TEMPDIR)
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{namespace}}|$(NAMESPACE)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{registry}}|$(REGISTRY)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{postgres_password}}|$(POSTGRES_PASSWORD_BASE64)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{redis_password}}|$(REDIS_PASSWORD_BASE64)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{version}}|$(VERSION)|g"
	kubectl apply -f $(TEMPDIR)
	rm -rf $(TEMPDIR)

deploy-registry:
	$(call kubectl_dir, apply, addons/kube-registry/)
deploy-monitoring:
	$(call kubectl_dir, apply, addons/kube-monitoring/)
deploy-logging:
	$(call kubectl_dir, apply, addons/kube-logging/)
deploy-kube-lego:
	$(call kubectl_dir, apply, addons/kube-lego/)
wait-kube-lego:
	export kube_lego_ip=""; \
	while : ; do \
		kube_lego_ip=$$(kubectl get svc nginx --namespace=nginx-ingress --template="{{range .status.loadBalancer.ingress}}{{.ip}}{{end}}"); \
		[ -z "$${kube_lego_ip}" ] || break; sleep 5; \
	done

set-cf-ip:
	kube_lego_ip=$$(kubectl get svc nginx --namespace=nginx-ingress --template="{{range .status.loadBalancer.ingress}}{{.ip}}{{end}}"); \
	[ ! -z "$${kube_lego_ip}" ] && which cfcli &>/dev/null && cfcli -t A editrecord polykube.io $${kube_lego_ip} || true

clean-all: | clean-registry clean-monitoring clean-logging clean-kube-lego clean-polykube
clean-registry:
	$(call kubectl_dir, delete, addons/kube-registry/) || true
clean-monitoring:
	$(call kubectl_dir, delete, addons/kube-monitoring/) || true
clean-logging:
	$(call kubectl_dir, delete, addons/kube-logging/) || true
clean-kube-lego:
	kubectl delete ns kube-lego
	kubectl delete ns nginx-ingress
clean-polykube:
	kubectl delete ns $(NAMESPACE)

define kubectl_dir
	find $(2) -type f -name '*.yaml' | sort | while read line; do \
		kubectl $(1) -f $$line; \
	done
endef
