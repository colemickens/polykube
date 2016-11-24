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

1. `make`
2. `docker`
3. `kubectl` ([linux: amd64](https://storage.googleapis.com/kubernetes-release/release/v1.4.6/bin/linux/amd64/kubectl)) ([darwin: amd64](https://storage.googleapis.com/kubernetes-release/release/v1.4.6/bin/darwin/amd64/kubectl)) ([windows: amd64](https://storage.googleapis.com/kubernetes-release/release/v1.4.6/bin/windows/amd64/kubectl.exe))

## Principles

 * Anyone can hack on this project, with a single command
   `./devenv.sh` (or even `.\devenv.ps1`)
   This command drops you in a development environment with any and all
   SDKs installed, ready to use.
 * Each project requiring a real build, can be built in the development
   environment with `./build.sh` in the project directory.
 * All container image builds are doone by first performing the builds are done inside containers for portability and reproducability
 * Final container images are built by first doing a containerized build of the project,
   and then placing the build output into the final container image.
 * All containers used in the service are as minimal as possible.
   No development-time dependencies are included.
   For example, this means that the final AspNetCore containers do not have the SDK
   and tooling installed in them. This reduces image size and speeds up deployment.

## Layout

Each component has, where applicable:
- a `make dev` command to enter a development environment ready to immediately build/run/test
- a `make dbuild` command to build using a container
- a `make container` command to build the "production" container that will be deploy
- a `make push` command to push the image to the registry, specified by `$REGISTRY`

## Deployment (Automatic)

This will perform all steps automatically:

```shell
export CLUSTER_NAME=colemick-polykube1
./magic.sh
```

## Deployment (Manual)

I recommend you explore the rabbit hole of Makefiles. It helps explain how pieces fit together.

## TODO
  0. Finish the frontend. (Angular2)
  1. move dotnet dns hack out of kube deploy files and into C#


---

# OLD README

---

## Features
  0. **Docker:** Minimal production containers including only the application (no source code, no build tools, etc)

  1. **Kubernetes:** Fast, reliable and repeatable deployments using Kubernetes Deployments, Services, DaemonSets and Secrets.

  3. **.NET Core:** Example AspNetCore API web application.

  4. **Angular2:**  Example `angular2-material` frontend Typescript application

## Goals

  1. Provide an example of how to use Docker for local development. This repo shows how you can use docker to have a full development environment that only relies on `make` and `docker` being available.

  2. Provide an example of how to build Docker containers meant for production.

  2. Provide an example of using Kubernetes for production container management,


## Motivations

  1. Encourage Docker adoption, even for software that doesn't need to be run with Docker. The fact that anyone can install `git`, `make`, `docker`, and clone my repository and instantly have a working development environment is a big deal. I can't imagine the number of projects that I've fiddled with for hours before abandoning because I couldn't get the right version of `node`/`npn`/dependenics and the application code to get along together. These issues largely disappear if there is a `Dockerfile` that defines the development environment for the project.

  2. Encourage better Docker practices. Your production service doesn't need a compiler. Your production service doesn't need a copy of your application source code. So why ship them and their bloat to your production environment? This repo includes an example of how one can have separate `build` and `runtime` containers so that you can ship only your final binary bits (and their dependencies) to prodction.

  3. Encourage Kubernetes adoption. I continue to see people who think they don't need the "complexity" of Kubernetes, and then wind up re-inventing the so-called complexity via bash scripts that are subject to less design and review than the Kubernetes project. Hopefully this repo can demonstrate the few yaml files needed to make a project deployable into Kubernetes.


## Kubernetes features

0. Basics

   Uses Deployments and Services to abstract and manage your actual application.

1. Secrets

   Uses Secrets to provide database and redis passwords at runtime. This eliminates the need for application secrets to ever live in the application source code repository.

2. Service Discovery

   Uses the Cluster DNS addon to resolve services at runtime in cluster.
   (The dotnet api simply connects to postgres via `tcp://db:5432`, redis via `tcp://redis:6379`, etc.)
