#!/usr/bin/env bash

set -eux

npm install
ng build -prod
rm -rf ../dist
cp -a dist ../dist
