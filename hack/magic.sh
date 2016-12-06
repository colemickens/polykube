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
export NAMEOVERRIDE="${NAMEOVERRIDE:-polykube}"
export VERSION="${VERSION:-$(git describe --dirty --abbrev=10 --always)}"

###############################################################################

startup() {
	#export WORKDIR="$(mktemp -d /tmp/tmp.polykube.XXXXXXXX)"
	export WORKDIR="${DIR}/_deployments/polykube.${VERSION}"
	mkdir -p "${WORKDIR}"
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
export ACS_NAME="${NAME//-/}"
az acs show --name "${ACS_NAME}" --resource-group="${RESOURCE_GROUP}" \
|| az acs create \
	--name="${ACS_NAME}" \
	--resource-group="${RESOURCE_GROUP}" \
	--orchestrator="kubernetes" \
	--dns-prefix="${ACS_NAME}" \
	--agent-vm-size="Standard_D2_v2" \
	--agent-count=2 \
	--service-principal="${CLIENT_ID}" \
	--client-secret="${CLIENT_SECRET}" \

# Create Azure Container Registry, if it doesn't exist
export ACR_NAME="${NAME//-/}"
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

# Docker login to ACR registry so we can push
# TODO: this modifies global state (~/.docker/config.json)
# TODO: see if there is something similar to what 'az' offers for isolation
docker login \
	--username "${CLIENT_ID}" \
	--password "${CLIENT_SECRET}" \
	"${REGISTRY}"

# Push polykube images to remote registry
# NOTE: This implicitly builds the production-ready containers before pushing
"${DIR}/build-push-all.sh"

# Make sure tiller is running
kubectl get deployment tiller-deploy --namespace kube-system \
|| (helm init && sleep 20)

(
	cd ${DIR}/../chart/
	helm install \
		./polykube \
		--set image.registry="${REGISTRY}",image.tag="${VERSION}",nameOverride="${NAMEOVERRIDE}",image.username="${CLIENT_ID}",image.password="${CLIENT_SECRET}"
)
