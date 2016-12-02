#!/usr/bin/env bash

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

set -e
set -u
set -x

[[ -f "${DIR}/user.env" ]] && source "${DIR}/user.env"

# User needs to specify these
export NAME="${NAME}"
export RESOURCE_GROUP="${RESOURCE_GROUP}"
export LOCATION="${LOCATION}"

export TENANT_ID="${TENANT_ID}"
export SUBSCRIPTION_ID="${SUBSCRIPTION_ID}"
export CLIENT_ID="${CLIENT_ID}"
export CLIENT_SECRET="${CLIENT_SECRET}"

# Optional (blank = skip auto DNS)
export DOMAIN="${DOMAIN:-}"

# Constants
export AZURE_CLI_IMAGE="azuresdk/azure-cli-python:0.1.0b10"
export AZ_CONTAINER="polykube-az"
export KUBECTL_IMAGE="gcr.io/google_containers/kubectl-amd64:v1.5.0-beta.1"
export KUBECTL_CONTAINER="polykube-kubectl"

# TODO: check if already exists and skip this step if so

cleanup() {
	docker stop "${AZ_CONTAINER}" || true
	docker rm "${AZ_CONTAINER}" || true
	docker stop "${KUBECTL_CONTAINER}" || true
	docker rm "${KUBECTL_CONTAINER}" || true
}

startup() {
	docker run -dt --name="${AZ_CONTAINER}" "${AZURE_CLI_IMAGE}"
	docker run -dt --name="${KUBECTL_CONTAINER}" "${KUBECTL_IMAGE}"
}

# Cleanup Now and Later
cleanup
trap cleanup EXIT

startup

docker exec "${AZ_CONTAINER}" \
	az login --service-principal \
		--username="${CLIENT_ID}" \
		--password="${CLIENT_SECRET}" \
		--tenant="${TENANT_ID}"

docker exec "${AZ_CONTAINER}" \
	az account set --subscription="${SUBSCRIPTION_ID}"

docker exec "${AZ_CONTAINER}" \
	az account show

docker exec "${AZ_CONTAINER}" \
	az resource group show --name="${RESOURCE_GROUP}" \
	|| az resource group create \
		--name="${RESOURCE_GROUP}" \
		--location="${LOCATION}"

export ACR_NAME="${NAME}registry"
export ACR_NAME="${ACR_NAME//-/}"
docker exec "${AZ_CONTAINER}" \
	az acr show --name="${ACR_NAME}" --resource-group="${RESOURCE_GROUP}" \
	|| az acr create \
		--name="${ACR_NAME}" \
		--resource-group="${RESOURCE_GROUP}" \
		--location="${LOCATION}"

export ACS_NAME="${NAME}"
export ACS_DNS_PREFIX="${NAME}${RANDOM}"
export KUBECONFIG="$(mktemp -d)/kubeconfig.json"
docker exec "${AZ_CONTAINER}" \
	az acs show --name "${ACS_NAME}" --resource-group="${RESOURCE_GROUP}" \
	|| az acs create \
		--name="${ACS_NAME}" \
		--resource-group="${RESOURCE_GROUP}" \
		--orchestrator="kubernetes" \
		--dns-prefix="${ACS_DNS_PREFIX}" \
		--agent-vm-size="Standard_D4_v2" \
		--agent-count=5 \
		--service-principal="${CLIENT_ID}" \
		--client-secret="${CLIENT_SECRET}" \

# copy KUBECONFIG out of the az container
docker exec "${AZ_CONTAINER}" \
	mkdir -p '/root/.kube' \
	&& az acs kubernetes get-credentials \
		--dns-prefix="${ACS_DNS_PREFIX}" \
docker cp "${AZ_CONTAINER}:/root/.kube/config" "${KUBECONFIG}"

# copy KUBECONFIG into the kubectl container
docker exec "${KUBECTL_CONTAINER}" mkdir -p '/root/.kube'
docker cp "${KUBECONFIG}" "${KUBECTL_CONTAINER}:/root/.kube/config"

# create imagePullSecret for ACR
docker exec "${KUBECTL_CONTAINER}" \
	kubectl create secret docker-registry \
		acr-registry-secret \
		--docker-server="${ACR_NAME}.azure-containers.io" \
		--docker-username="${CLIENT_ID}" \
		--docker-password="${CLIENT_SECRET}" \
		--docker-email="notapplicable@example.com"

# push some shit to the remote registry
# deploy some shit to the ACS kube cluster

# deploy ACR registry
# grab credentials

# deploy ACS cluster
# grab credentials

# inject imagePullSecret into cluster

# hack/build-push-all.sh

# deploy local helm chart
