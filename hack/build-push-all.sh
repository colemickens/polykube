#!/usr/bin/env bash

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

ROOT="${DIR}/.."
LOGDIR="$(mktemp -d)"

echo "Build logs are in ${LOGDIR}"

# TODO: this is a bit hacky, would like to do something different
# especially so it's possible to get an idea of how long each build really takes
for dir in "${ROOT}/polykube-"*/; do
  cd "${dir}";
  logname="$(basename "${dir}").log"
  make push &> "${LOGDIR}/${logname}" &
  cd ..
done
wait
