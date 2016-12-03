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

###############################################################################

ROOT="${DIR}/.."
LOGDIR="$(mktemp -d)"

if [[ "${REGISTRY:-}" == "" ]]; then
  echo "REGISTRY must be set"
  exit -1
fi

echo "Build logs are in ${LOGDIR}"

for dir in "${ROOT}/polykube-"*/; do
  cd "${dir}";
  logname="$(basename "${dir}").log"
  make push
  cd ..
done
