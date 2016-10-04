# polykube

This application serves an example of a full-stack application built for Kubernetes.

See [demo](DEMO.md) for instructions on deploying this application to a Kubernetes cluster.

The application is running at [polykube.io](http://polykube.io) and [api.polykube.io](http://api.polykube.io/counter).

## TODO

  0. Finish the frontend. (Angular2)


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

## Fine Print

  * This is a work-in-progress
  * I will be resetting `master` and the repo history regularly
