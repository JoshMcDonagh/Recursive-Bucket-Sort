.PHONY: cpython
cpython:
	docker build -f ./docker-CPython --tag benchmark-cpython .
	docker run -v ${PWD}:/app/ -e OUTPUT=/app/results.json benchmark-cpython
	mv ./results.json ./cpython.json || move ./results.json ./cpython.json

.PHONY: pypy
pypy:
	docker build -f ./docker-Pypy --tag benchmark-pypy .
	docker run -v ${PWD}:/app/ -e OUTPUT=/app/results.json benchmark-pypy
	mv ./results.json ./pypy.json || move ./results.json ./pypy.json

.PHONY: both
both: cpython pypy