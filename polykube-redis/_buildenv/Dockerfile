FROM docker.io/redis:3.2

CMD bash -c 'export REDIS_PASSWORD="$(cat /etc/polykube/redis-auth/password | head -n 1)"; /usr/local/bin/docker-entrypoint.sh redis-server --requirepass ${REDIS_PASSWORD}'
