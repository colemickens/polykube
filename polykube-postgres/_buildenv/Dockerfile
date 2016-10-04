FROM docker.io/postgres:9.6

CMD bash -c 'export POSTGRES_PASSWORD="$(cat /etc/polykube/postgres-auth/password | head -n 1)"; /docker-entrypoint.sh postgres'
