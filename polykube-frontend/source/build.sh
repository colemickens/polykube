#!/usr/bin/env bash

set -eux

cd polykube-frontend
npm install
ng build -prod
rm -rf ../dist
cp -a dist ../../dist
