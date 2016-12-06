#!/usr/bin/env bash

set -eux

cd polykube-frontend
npm install
ng build -prod
rm -rf ../../dist
pwd
ls -la .
ls -la ..
ls -la ../..
cp -a dist ../../dist/
