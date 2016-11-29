#!/usr/bin/env bash

set -eux

cd polykube-frontend
ng serve \
	--host 0.0.0.0 \
	--port 9999 \
	--live-reload-host 0.0.0.0 \
	--live-reload-port 9998
