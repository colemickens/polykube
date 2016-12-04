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

###############################################################################

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

###############################################################################

startup() {
	export WORKDIR="$(mktemp -d /tmp/tmp.polykube.XXXXXXXX)"
}

cleanup() {
	set +x
	echo
	echo "******** WARNING ********"
	echo "** ${WORKDIR} contains sensitive assets. Delete or backup as needed."
	echo "******** WARNING ********"
}

startup
trap cleanup EXIT

echo "******** WORKDIR: ${WORKDIR} ********"

# Isolate our changes
export AZURE_CONFIG_DIR="${WORKDIR}/az-cli-config"

# Login
az login --service-principal \
	--username="${CLIENT_ID}" \
	--password="${CLIENT_SECRET}" \
	--tenant="${TENANT_ID}"

# Set subscription
az account set --subscription="${SUBSCRIPTION_ID}"

# Create resource group, if it doesn't exist
az resource group show --name="${RESOURCE_GROUP}" \
|| az resource group create \
	--name="${RESOURCE_GROUP}" \
	--location="${LOCATION}"

# Create Kubernetes cluster on Azure Container Service, if it doesn't exist
export ACS_NAME="${NAME}"
export ACS_DNS_PREFIX="${NAME}${RANDOM}"
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

# Create Azure Container Registry, if it doesn't exist
export ACR_NAME="${NAME}registry"
export ACR_NAME="${ACR_NAME//-/}"
export REGISTRY="${ACR_NAME}-microsoft.azurecr.io"
az acr show --name="${ACR_NAME}" --resource-group="${RESOURCE_GROUP}" \
|| az acr create \
	--name="${ACR_NAME}" \
	--resource-group="${RESOURCE_GROUP}" \
	--location="${LOCATION}"

# Download KUBECONFIG for the cluster
export KUBECONFIG="${WORKDIR}/kubeconfig"
az acs kubernetes get-credentials \
	--name "${ACS_NAME}" \
	--resource-group "${RESOURCE_GROUP}" \
	--file="${KUBECONFIG}"
	#--dns-prefix="${ACS_DNS_PREFIX}" \
	#--location="${LOCATION}" \
	#--file="${KUBECONFIG}"

# create imagePullSecret for ACR
kubectl get secret acr-registry-secret \
|| kubectl create secret docker-registry \
	acr-registry-secret \
	--docker-server="${ACR_NAME}.azure-containers.io" \
	--docker-username="${CLIENT_ID}" \
	--docker-password="${CLIENT_SECRET}" \
	--docker-email="notapplicable@example.com"

# Docker login to ACR registry so we can push
# TODO: this modifies global state (~/.docker/config.json)
# TODO: see if there is something similar to what 'az' offers for isolation
docker login \
	--username "${CLIENT_ID}" \
	--password "${CLIENT_SECRET}" \
	"${REGISTRY}"

# Push polykube images to remote registry
# NOTE: This implicitly builds the production-ready containers before pushing
export VERSION=stable
"${DIR}/build-push-all.sh"

(
	cd ${DIR}/../chart/
	helm package polykube
	helm deploy ./polykube.tar.gz
)
