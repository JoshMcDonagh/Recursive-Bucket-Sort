# Overview

This is the source code for the backend server.

This is a web API programmed in F#.

# Tech Stack

* ASP Core
* EF Core - (A relational database is the *wrong* choice here, but I wanted to use something I know to get it done fast)
* Giraffe

# Developing

In one terminal, run `make docker-dev` using the Makefile in the parent folder.

In another terminal, run `make backend-dev`, and any changes will cause a rebuild of the project automatically.