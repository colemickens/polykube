# polykube

## Overview

This repository is an example of a "full stack application" (a *very* simple Guest Book), which is ready to be deployed into a Kubernetes cluster.

The `README` walks through deployment of:

  * a [Kubernetes](http://kubernetes.io/) cluster on the [Azure Container Service](https://azure.microsoft.com/en-us/services/container-service/)
  * a private Docker registry hosted by [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/)

Next, the service's components are deployed on the Kubernetes cluster:

  * an [AspNetCore](https://docs.microsoft.com/en-us/aspnet/core/) application powered by [.NET Core](https://www.microsoft.com/net/core)
  * a [Redis](http://redis.io/) instance, used as a cache in the AspNetCore application
  * a(n) [SQL Server for Linux](https://www.microsoft.com/en-us/sql-server/sql-server-vnext-including-Linux) instance, used a the persistent store for the AspNetCore application
  * an [Angular2 frontend](https://angular.io/) with [Material UI components](https://material.angular.io/)
  * a Kubernetes [Ingress object] with a [Service]() that will result in the application being available to the Internet via
    an [Azure Load Balancer](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-overview) and `nginx` (via the [nginx-ingress-controller]()).

## Demo

~~The application is running at [polykube.io](https://polykube.io) and [api.polykube.io](https://api.polykube.io/counter).~~

~~This is an [Asciinema presentation]() that shows these steps, from scratch, in realtime.
From having *nothing* deployed in Azure, to having this exposed to the world takes less than 15 minutes:~~

~~`[ insert asciinema screenshot link here ]`~~

~~There is also [a less real-time version with the waits removed]().~~

## Prerequisites

0. Linux
1. `make`
2. `docker`
3. `kubectl` ([linux: amd64](https://storage.googleapis.com/kubernetes-release/release/v1.4.6/bin/linux/amd64/kubectl)) ([darwin: amd64](https://storage.googleapis.com/kubernetes-release/release/v1.4.6/bin/darwin/amd64/kubectl)) ([windows: amd64](https://storage.googleapis.com/kubernetes-release/release/v1.4.6/bin/windows/amd64/kubectl.exe))
4. `az`, the [Azure CLI](https://github.com/Azure/azure-cli), aka, *"the new Python Azure CLI"*, not the nodejs, azure-xplat-cli.

## Principles

 * Anyone can hack on this project, with a single command
   `./devenv.sh` (or even `.\devenv.ps1`)
   This command drops you in a development environment with any and all
   SDKs installed, ready to use.
 * Each project requiring a real build, can be built in the development
   environment with `./build.sh` in the project directory.
 * All container image builds are done by first performing the builds are done inside containers for portability and reproducability
 * Final container images are built by first doing a containerized build of the project,
   and then placing the build output into the final container image.
 * Final containers created for the service are as minimal as possible.
   (The dotnet api simply connects to postgres via `tcp://db:5432`, redis via `tcp://redis:6379`, etc.)
   No development-time dependencies are included.
   For example, this means that the final AspNetCore containers do not have the SDK
   and tooling installed in them. This reduces image size and speeds up deployment.
 * Respect the ["docker build container pattern"](http://blog.slashdeploy.com/2016/11/07/docker-build-container-pattern/).
   As a summary, this means that containerized build output is owned by the correct user (not root)
   and it means the containerized final build process can be run even in configurations where the `docker` daemon is
   running remotely (like a really powerful remote Azure VM).

## Deployment (Automatic)

This will perform all steps automatically:

```shell
export CLUSTER_NAME=colemick-polykube1
./magic.sh
```

With expected output:
```shell
# output:
# ...
# `polykube` is live at: http://1.2.3.4
```

## Deployment (Manual)

(For now, read `./magic.sh` and see how it works.)

## Todo
  0. Finish the frontend for the third time. (Angular2)
  1. move dotnet dns hack out of kube deploy files and into C#

