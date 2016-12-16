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
export VERSION="${VERSION:-$(git describe --dirty --always)}"

###############################################################################

startup() {
	export WORKDIR="${DIR}/_deployments/${NAME}"
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
az group show --name="${RESOURCE_GROUP}" \
|| az group create \
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
# (note this intentionally comes after ACR so the
#  nodes can finish provisioning...)
export KUBECONFIG="${WORKDIR}/kubeconfig"
rm -f "${KUBECONFIG}" # todo, file a bug on azure-cli for not removing after merge
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
|| (helm init && sleep 15)

cat <<-EOF > "${WORKDIR}/values.yaml"
	image:
	  registry: "${REGISTRY}"
	  tag: "${VERSION}"
	  username: "${CLIENT_ID}"
	  password: "${CLIENT_SECRET}"
	domain: "${DOMAIN}"
	nameOverride: "${NAMEOVERRIDE}"
EOF

HELM_RELEASE="polykube-release0"
if ! helm get "${HELM_RELEASE}" &>/dev/null ; then
	helm install "${DIR}/../chart/polykube" --name "${HELM_RELEASE}" --values "${WORKDIR}/values.yaml"
else
	helm upgrade "${HELM_RELEASE}" "${DIR}/../chart/polykube" --values "${WORKDIR}/values.yaml"
fi

# TODO: parameterize the project name here
external_ip=""
while true; do
	external_ip=$(kubectl get svc --namespace polykube nginx-ingress --template="{{range .status.loadBalancer.ingress}}{{.ip}}{{end}}")
	[[ ! -z "${external_ip}" ]] && break
	sleep 10
done

if [[ "${DNS_RESOURCE_GROUP:-}" != "" ]]; then
	az network dns record-set delete \
		--resource-group="${DNS_RESOURCE_GROUP}" \
		--zone-name="${DNS_ZONE_NAME}" \
		--type='A' \
		--name="${DNS_RECORD_SET_NAME}" \
			|| true

	az network dns record-set create \
		--resource-group="${DNS_RESOURCE_GROUP}" \
		--zone-name="${DNS_ZONE_NAME}" \
		--type='A' \
		--ttl=120 \
		--name="${DNS_RECORD_SET_NAME}"

	az network dns record a add \
		--resource-group="${DNS_RESOURCE_GROUP}" \
		--zone-name="${DNS_ZONE_NAME}" \
		--record-set-name="${DNS_RECORD_SET_NAME}" \
		--ipv4-address="${external_ip}"
fi

echo "https://${DOMAIN}"
