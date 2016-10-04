NOTPARALLEL:

all:
	echo "please specify a command"
	false

prep:
	$(eval TEMPDIR = $(shell mktemp -d))
	$(eval VERSION ?= $(shell git describe --always --dirty))
	$(eval NAMESPACE ?= polykube)
	$(eval REGISTRY ?= localhost:5000)
	$(eval export VERSION)
	$(eval export NAMESPACE)
	$(eval export REGISTRY)

prep-deploy: prep
	$(eval REDIS_PASSWORD ?= $(shell cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 32 | head -n 1 | base64 -w 0))
	$(eval POSTGRES_PASSWORD ?= $(shell cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 32 | head -n 1 | base64 -w 0))
	cp -r * $(TEMPDIR)/
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{namespace}}|$(NAMESPACE)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{repo}}|$(REGISTRY)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{postgres_password}}|$(POSTGRES_PASSWORD)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{redis_password}}|$(REDIS_PASSWORD)|g"
	find "$(TEMPDIR)" -name "*.yaml" -print0 | xargs -0 sed -i -e "s|{{version}}|$(VERSION)|g"

deploy-all: | deploy-registry deploy-monitoring deploy-logging deploy-kube-lego deploy-polykube

deploy-registry:
	# TODO: follow up and see if we can use an "upstream" daemonset for Kube-Registry
	kubectl apply -f https://gist.githubusercontent.com/colemickens/b76c286ec3dba52634d7c897f56d6cbc/raw/3cbd376a2555966fc451f99fdbbb26a912b702a5/kube-registry.yaml

deploy-monitoring:
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/heapster/master/deploy/kube-config/influxdb/grafana-service.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/heapster/master/deploy/kube-config/influxdb/heapster-service.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/heapster/master/deploy/kube-config/influxdb/influxdb-service.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/heapster/master/deploy/kube-config/influxdb/heapster-controller.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/heapster/master/deploy/kube-config/influxdb/influxdb-grafana-controller.yaml

deploy-logging:
	# TODO: follow up and see if we can use an "upstream" daemonset for FluentD
	kubectl apply -f https://gist.githubusercontent.com/colemickens/68cc04a19ed834c3f038cba0959e9e40/raw/9ef32f28efae8cbe0839c83a3d467fb1c77b13bc/fluentd-elasticsearch-v1-daemonset.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/kubernetes/master/cluster/addons/fluentd-elasticsearch/es-service.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/kubernetes/master/cluster/addons/fluentd-elasticsearch/es-controller.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/kubernetes/master/cluster/addons/fluentd-elasticsearch/kibana-service.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/kubernetes/master/cluster/addons/fluentd-elasticsearch/kibana-controller.yaml

deploy-polykube: prep-deploy clean-polykube
	kubectl apply -f $(TEMPDIR)/namespace.yaml
	kubectl apply -f $(TEMPDIR)/redis.yaml
	kubectl apply -f $(TEMPDIR)/postgres.yaml
	kubectl apply -f $(TEMPDIR)/frontend.yaml
	kubectl apply -f $(TEMPDIR)/aspnet-api.yaml
	kubectl apply -f $(TEMPDIR)/ingress.yaml

deploy-kube-lego:
	#kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/05-echoserver-namespace.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/05-kube-lego-namespace.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/05-nginx-ingress-namespace.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/20-default-http-backend-svc.yaml
	#kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/20-echoserver-svc.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/20-kube-lego-configmap.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/20-nginx-configmap.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/20-nginx-svc.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/50-default-http-backend-deployment.yaml
	#kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/50-echoserver-deployment.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/50-kube-lego-deployment.yaml
	kubectl apply -f https://raw.githubusercontent.com/jetstack/kube-lego/master/examples/nginx/50-nginx-deployment.yaml

clean-polykube: prep
	kubectl delete --namespace $(NAMESPACE) deployment aspnet-api frontend postgres redis || true
.NOTPARALLEL:

## experimental

magic-polykube: | registry-start deploy-polykube registry-stop

registry-start:
	$(eval REGISTRY_POD_NAME = $(shell kubectl get pods --selector=k8s-app=kube-registry --namespace=kube-system --output=jsonpath="{.items[0].metadata.name}"))
	kubectl port-forward --namespace=kube-system ${REGISTRY_POD_NAME} 5000 & >/dev/null

registry-stop:
	$(eval PID = $(shell pgrep -a kubectl | grep "port-forward --namespace=kube-system kube-registry-" | grep "5000" | cut -d' ' -f1))
	kill $(PID)

