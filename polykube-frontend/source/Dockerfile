FROM buildpack-deps:xenial-scm

RUN curl -sfL https://deb.nodesource.com/setup_6.x | bash -
RUN apt-get update -qq \
	&& apt-get install -qqy nodejs build-essential autoconf automake python-dev \
	&& rm -rf /var/lib/apt/lists/*

### workaround bug that breaks watchman installed via npm
#RUN bash -c "\
#	git clone https://github.com/facebook/watchman.git; \
#	cd watchman; \
#	git checkout v4.5.0; \
#	./autogen.sh; \
#	./configure; \
#	make; \
#	make install;"

RUN npm install -g angular-cli
RUN npm install -g npm-check-updates

ADD . /polykube-frontend/source
WORKDIR /polykube-frontend/source

ARG BUILD=${BUILD:-n}
RUN bash -c "if [[ \"${BUILD}\" == \"y\" ]]; then ./build.sh; fi"

CMD /usr/bin/env bash
