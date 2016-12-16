FROM buildpack-deps:xenial-scm

RUN apt-get update -qq && apt-get install apt-transport-https && rm -rf /var/lib/apt/lists/*
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add - \
	&& echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list \
	&& curl -sfL https://deb.nodesource.com/setup_6.x | bash - \
	&& apt-get install -qqy yarn \
	&& apt-get install -qqy nodejs build-essential autoconf automake python-dev \
	&& rm -rf /var/lib/apt/lists/*

RUN npm install -g npm-check-updates
RUN yarn global add vue-cli

ADD . /polykube-frontend/source
WORKDIR /polykube-frontend/source

ARG BUILD=${BUILD:-n}
RUN bash -c "if [[ \"${BUILD}\" == \"y\" ]]; then ./build.sh; fi"

CMD /usr/bin/env bash
