.PHONY: help restore build run clean publish docker-build docker-up docker-down docker-restart migrate migrate-update migrate-reset seed test format lint

# Variables
SOLUTION_FILE = ExpenseTracker.sln
API_PROJECT = src/ExpenseTrackerApi/ExpenseTrackerApi.csproj
CONFIGURATION = Debug
DOCKER_COMPOSE = docker-compose.yml

# Colors for output
CYAN = \033[0;36m
GREEN = \033[0;32m
YELLOW = \033[0;33m
RED = \033[0;31m
NC = \033[0m # No Color

# Default target
.DEFAULT_GOAL := help

##@ General

help: ## Display this help message
	@echo "$(CYAN)Expense Tracker - Available Commands:$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  $(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

##@ Build

restore: ## Restore NuGet packages
	@echo "$(CYAN)Restoring NuGet packages...$(NC)"
	dotnet restore $(SOLUTION_FILE)

build: restore ## Build the solution
	@echo "$(CYAN)Building solution...$(NC)"
	dotnet build $(SOLUTION_FILE) --configuration $(CONFIGURATION) --no-restore

build-release: restore ## Build the solution in Release mode
	@echo "$(CYAN)Building solution in Release mode...$(NC)"
	dotnet build $(SOLUTION_FILE) --configuration Release --no-restore

publish: ## Publish the API project
	@echo "$(CYAN)Publishing API project...$(NC)"
	dotnet publish $(API_PROJECT) --configuration Release --output ./publish

##@ Run

run: ## Run the API locally
	@echo "$(CYAN)Running API locally...$(NC)"
	dotnet run --project $(API_PROJECT)

run-watch: ## Run the API with hot reload
	@echo "$(CYAN)Running API with hot reload...$(NC)"
	dotnet watch run --project $(API_PROJECT)

##@ Database

migrate: ## Create a new migration (usage: make migrate NAME=migration-name)
	@if [ -z "$(NAME)" ]; then \
		echo "$(RED)Error: Migration name is required. Usage: make migrate NAME=migration-name$(NC)"; \
		exit 1; \
	fi
	@echo "$(CYAN)Creating migration: $(NAME)...$(NC)"
	dotnet ef migrations add $(NAME) --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT)

migrate-update: ## Manually apply pending migrations to the database
	@echo "$(CYAN)Applying migrations to database...$(NC)"
	dotnet ef database update --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT)
	@echo "$(GREEN)Migrations applied successfully!$(NC)"

migrate-remove: ## Remove the last migration (usage: make migrate-remove)
	@echo "$(YELLOW)Removing last migration...$(NC)"
	dotnet ef migrations remove --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT)

migrate-list: ## List all migrations
	@echo "$(CYAN)Listing migrations...$(NC)"
	dotnet ef migrations list --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT)

migrate-reset: ## Reset migrations: drop database, remove all migrations, and create fresh initial migration (usage: make migrate-reset NAME=InitialCreate)
	@echo "$(YELLOW)WARNING: This will drop the database and remove all migrations!$(NC)"
	@read -p "Are you sure you want to continue? (y/N) " -n 1 -r; \
	echo; \
	if [[ ! $$REPLY =~ ^[Yy]$$ ]]; then \
		echo "$(RED)Operation cancelled.$(NC)"; \
		exit 1; \
	fi
	@echo "$(CYAN)Dropping database...$(NC)"
	@dotnet ef database drop --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT) --force || true
	@echo "$(CYAN)Removing all migrations...$(NC)"
	@find src/ExpenseTracker.Infrastructure/Data/Migrations -name "*.cs" ! -name "*Designer.cs" -type f -delete || true
	@find src/ExpenseTracker.Infrastructure/Data/Migrations -name "*.Designer.cs" -type f -delete || true
	@if [ -z "$(NAME)" ]; then \
		echo "$(CYAN)Creating fresh initial migration...$(NC)"; \
		dotnet ef migrations add InitialCreate --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT) --output-dir Data/Migrations;\
	else \
		echo "$(CYAN)Creating fresh migration: $(NAME)...$(NC)"; \
		dotnet ef migrations add $(NAME) --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT) --output-dir Data/Migrations; \
	fi
	@echo "$(CYAN)Applying migrations to database...$(NC)"
	@dotnet ef database update --project src/ExpenseTracker.Infrastructure --startup-project $(API_PROJECT)
	@echo "$(GREEN)Migration reset completed!$(NC)"

seed: ## Manually seed the database
	@echo "$(CYAN)Seeding database...$(NC)"
	@dotnet run --project $(API_PROJECT) -- --seed
	@echo "$(GREEN)Database seeding completed!$(NC)"

##@ Docker

docker-build: ## Build Docker image
	@echo "$(CYAN)Building Docker image...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) build

docker-up: ## Start all Docker containers
	@echo "$(CYAN)Starting Docker containers...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) up -d

docker-down: ## Stop all Docker containers
	@echo "$(CYAN)Stopping Docker containers...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) down

docker-restart: docker-down docker-up ## Restart all Docker containers

docker-logs: ## View Docker container logs
	@echo "$(CYAN)Viewing Docker logs...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) logs -f

docker-logs-api: ## View API container logs
	@echo "$(CYAN)Viewing API logs...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) logs -f api

docker-logs-db: ## View database container logs
	@echo "$(CYAN)Viewing database logs...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) logs -f sqlserver

docker-ps: ## List running Docker containers
	@echo "$(CYAN)Running containers:$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) ps

docker-shell-api: ## Open shell in API container
	@echo "$(CYAN)Opening shell in API container...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) exec api /bin/bash

docker-shell-db: ## Open shell in database container
	@echo "$(CYAN)Opening shell in database container...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) exec sqlserver /bin/bash

docker-clean: ## Remove containers, networks, and volumes
	@echo "$(YELLOW)Removing Docker containers, networks, and volumes...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE) down -v

##@ Code Quality

format: ## Format code using dotnet format
	@echo "$(CYAN)Formatting code...$(NC)"
	dotnet format $(SOLUTION_FILE)

lint: ## Run code analysis
	@echo "$(CYAN)Running code analysis...$(NC)"
	dotnet build $(SOLUTION_FILE) --configuration $(CONFIGURATION) --no-restore /p:TreatWarningsAsErrors=false

##@ Cleanup

clean: ## Clean build artifacts
	@echo "$(CYAN)Cleaning build artifacts...$(NC)"
	dotnet clean $(SOLUTION_FILE) --configuration $(CONFIGURATION)
	@echo "$(GREEN)Clean completed!$(NC)"

clean-all: clean ## Clean build artifacts and remove publish folder
	@echo "$(CYAN)Removing publish folder...$(NC)"
	rm -rf ./publish
	@echo "$(GREEN)Clean all completed!$(NC)"

##@ Development

dev-setup: restore build docker-up migrate-update ## Complete development setup
	@echo "$(GREEN)Development environment setup complete!$(NC)"
	@echo "$(CYAN)You can now run 'make run' to start the API locally$(NC)"

dev-reset: docker-down docker-clean docker-up migrate-update ## Reset development environment
	@echo "$(GREEN)Development environment reset complete!$(NC)"

##@ Utilities

check-dotnet: ## Check if .NET SDK is installed
	@echo "$(CYAN)Checking .NET SDK version...$(NC)"
	@dotnet --version || (echo "$(RED).NET SDK is not installed!$(NC)" && exit 1)

check-docker: ## Check if Docker is installed and running
	@echo "$(CYAN)Checking Docker...$(NC)"
	@docker --version || (echo "$(RED)Docker is not installed!$(NC)" && exit 1)
	@docker ps > /dev/null 2>&1 || (echo "$(RED)Docker is not running!$(NC)" && exit 1)

info: ## Display project information
	@echo "$(CYAN)Project Information:$(NC)"
	@echo "  Solution: $(SOLUTION_FILE)"
	@echo "  API Project: $(API_PROJECT)"
	@echo "  Configuration: $(CONFIGURATION)"
	@echo ""
	@echo "$(CYAN).NET SDK:$(NC)"
	@dotnet --version || echo "$(RED)Not installed$(NC)"
	@echo ""
	@echo "$(CYAN)Docker:$(NC)"
	@docker --version || echo "$(RED)Not installed$(NC)"

