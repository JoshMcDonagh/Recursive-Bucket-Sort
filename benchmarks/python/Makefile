.PHONY: cpython
cpython:
	docker build -f ./docker-CPython --tag benchmark-cpython .
	docker run -v ${PWD}:/app/ -e OUTPUT=/app/results.csv benchmark-cpython
	mv ./results.csv ./cpython.csv || move ./results.csv ./cpython.csv

.PHONY: pypy
pypy:
	docker build -f ./docker-Pypy --tag benchmark-pypy .
	docker run -v ${PWD}:/app/ -e OUTPUT=/app/results.csv benchmark-pypy
	mv ./results.csv ./pypy.csv || move ./results.csv ./pypy.csv

.PHONY: both
both: cpython pypy