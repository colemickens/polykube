#!/usr/bin/env bash

set -eux

cd polykube-frontend
yarn
npm run build
rm -rf ../../dist
cp -a dist ../../dist/
