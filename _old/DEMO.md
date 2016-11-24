# Polykube Demo
  This demo will walk you through deploying [`polykube`](https://github.com/colemickens/polykube).
  This demo will specifically cater to users who are following from [`azure-kubernetes-demo`](https://github.com/colemickens/azure-kubernetes-demo).

  The live version is visible at [polykube.io](http://polykube.io) 
  and [api.polykube.io](http://api.polykube.io/counter).

## Prerequisites

  1. A Kubernetes 1.4 Cluster deployed via [`azure-kubernetes-demo`](https://github.com/colemickens/azure-kubernetes-demo).

  2. `kubectl` installed. Copy and paste this if you want it quickly:
     ```shell
     export OS=linux
     export ARCH=amd64
     export VERSION=v1.3.3

     mkdir -p ~/.local/bin
     wget http://storage.googleapis.com/kubernetes-release/release/${VERSION}/bin/${OS}/${ARCH}/kubectl -O ~/.local/bin/kubectl
     chmod +x ~/.local/bin/kubectl

     export PATH=$PATH:~/.local/bin
     ```

## Connect to your cluster

  The `kubernetes-anywhere` deployment drops a kubeconfig into a temporary dir. 

  The easiest way to utilize it is to set `KUBECONFIG` to point to the config file.
  On my system, I run this:

  ```shell
  export KUBECONFIG=/home/cole/code/kubernetes-anywhere/azure/phase1/.tmp/kubeconfig
  ```

  This will of course only persist for the duration of your shell session.

  Until `kubernetes-anywhere` supports multiple side-by-side deployment, you may want
  to stash a copy of kubeconfig somewhere so that you don't accidentally overwrite it.

## Clone `polykube`

  ```shell
  git clone https://github.com/colemickens/polykube
  cd polykube

  cd kubernetes # all the fun stuff is in the kubernetes/ directory
  ```

## Deploy `kube-system` Services

To deploy the core essential services:
* `kube-proxy` - enables service clusterIPs to operate in cluster (basically always required)
* `kube-dns` - enables service discovery via DNS (somewhat optional, but very standard)
* `kubernetes-dashboard` - a UI for your cluster (optional)
* `kube-registry` - a cluster local docker registry (optional, but essential for this demo and for private deployments)

```shell
make deploy-kube-system-core
```

To deploy monitoring:
* `influxdb`
* `grafana`
* `heapster`

```shell
make deploy-kube-system-monitoring
```

To deploy logging:
* `fluentd`
* `grafana`
* `heapster`

```shell
make deploy-kube-system-logging
```

Most of these are not specific to an Azure cluster, or specific to the `polykube` application.
The only exception are that `kube-dns` need to use the correct service `clusterIP` that `kubelet`
is configured to inject and `kube-proxy` needs access to the host's `kubeconfig` and needs the
Kubernetes master's private ip hardcoded.

You can dig into the `Makefile` for a better idea of what all is deployed. Virtually all of it
is from the official Kubernetes project templates.


## Port-forward to your cluster-local Docker Registry

When we deployed `kube-registry`, we got a local Docker registry in our cluster.
In order to push images into it, we need to forward a port from our local machine
to the port of the `kube-registry` pod in the cluster.

Fortunately, `kubectl` has a command built in just for this purpose.

```shell
# use label selectors and the jsonpath output formatter to get the `kube-registry` pod name
REGISTRY_POD_NAME="$(kubectl get pods --selector=k8s-app=kube-registry --namespace=kube-system --output=jsonpath="{.items[0].metadata.name}")"

# make port 5000 port-forward to 5000 on the cluster-local docker-registry
kubectl port-forward --namespace=kube-system ${REGISTRY_POD_NAME} 5000
```

## Deploy `polykube` 

Build `polykube` the components to the cluster-local registry via the 
`kubectl port-forward` tunnel.

```shell
# build polykube-aspnet-api
# build polykube-frontend
# build polykube-postgres
# build polykube-redis
# push them all to the cluser-local registry
# deploy the Deployments and Services for `polykube`
make deploy-polykube
```

## See polykube!

```shell
# see the external L4 LoadBalanced IP for the frontend
kubectl get svc --namespace=polykube frontend

# see the external L4 LoadBalanced IP for the aspnet-api
kubectl get svc --namespace=polykube aspnet-api
```

  You could then wire up DNS and have your instance working identically to the 
  one running at [polykube.io](http://polykube.io).

## Extra Credit (Automatic TLS)

This is probably not the right way to deploy `kube-lego` since it's main
Pod will fail until DNS is configured, but it requires the least amount of
changes and coordination in the `Makefile`.

```shell
make deploy-kube-lego
```

This will create all of the Services and Deployments you need to have SSL working.
This will provision a new L4 load balancer and public ip address pointing at a
Deployment of `nginx-ingress-controller`. . This will automatically be given a TLS
certificate/private-key from the `kube-lego` components, as soon as you have created
the DNS A record for the desired domain to point at the new public ip address.
