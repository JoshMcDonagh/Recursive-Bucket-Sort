.PHONY: new-migration
new-migration:
	cd backend && \
	export DB_CONN_STRING="Server=127.0.0.1;Port=5432;Database=test;User Id=test;Password=test;" && \
	dotnet ef migrations add ${NAME}

.PHONY: backend-dev
backend-dev:
	cd backend && \
	export DB_CONN_STRING="Server=127.0.0.1;Port=5432;Database=test;User Id=test;Password=test;" && \
	dotnet watch run

.PHONY: frontend-dev
frontend-dev:
	cd frontend && \
	npm run dev

.PHONY: docker-dev
docker-dev:
	cd docker/dev && \
	docker-compose up

.PHONY: upload-backend
upload-backend:
	cd terraform && \
	ansible-playbook -i hosts.ini playbook.yml -t backend,services

.PHONY: upload-frontend
upload-frontend:
	cd terraform && \
	ansible-playbook -i hosts.ini playbook.yml -t frontend