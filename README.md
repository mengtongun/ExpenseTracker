# Expense Tracker API

A production-ready ASP.NET Core Web API for tracking personal expenses, managing recurring charges, and generating comprehensive reports. Built with clean architecture principles, JWT authentication, and comprehensive validation.

## 🚀 Features

- **User Authentication & Authorization** - Secure JWT-based authentication with refresh token support
- **Expense Management** - Full CRUD operations for tracking expenses with categories
- **Category Management** - Customizable expense categories per user
- **Recurring Expenses** - Automate recurring charges with flexible scheduling (daily, weekly, monthly, yearly)
- **Advanced Reporting** - Generate summary reports with category breakdowns and time-based aggregations
- **Export Capabilities** - Export reports in CSV and PDF formats
- **Report History** - Archive and retrieve previously generated reports
- **Filtering & Search** - Filter expenses by category, date range, and amount
- **UUID-based Security** - Public UUID identifiers to prevent IDOR vulnerabilities

## 📋 Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Database Migrations](#database-migrations)
- [Database Seeding](#database-seeding)
- [Docker Deployment](#docker-deployment)
- [API Documentation](#api-documentation)
- [Development](#development)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

## 🏗️ Architecture

The solution follows a clean, layered architecture pattern:

```
ExpenseTracker.sln
├── src/
│   ├── ExpenseTrackerApi/            # ASP.NET Core entrypoint (controllers, middleware, DI)
│   ├── ExpenseTracker.Application/   # Application services, DTOs, validators, options
│   ├── ExpenseTracker.Domain/        # Entities, enums, shared abstractions
│   └── ExpenseTracker.Infrastructure/# EF Core DbContext, repositories, persistence services
└── README.md
```

Key libraries: ASP.NET Core 9, EF Core 9 (SQL Server), AutoMapper, FluentValidation, Serilog, QuestPDF, CsvHelper, BCrypt.

## Prerequisites

- .NET SDK 9.0
- SQL Server (LocalDB is fine for development)
- Entity Framework Core tools: `dotnet tool install --global dotnet-ef`
- Optional: Docker + Docker Compose (for containerized runs)
- Optional: Make (for using Makefile commands)

## Configuration

The API reads settings from `appsettings.json`, environment-specific JSON files, and environment variables. Critical sections:

- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `Jwt` — `Issuer`, `Audience`, `SigningKey`, `AccessTokenMinutes`, `RefreshTokenDays`
- `Serilog` — console logging configuration

Override settings via environment variables, e.g. `Jwt__SigningKey`, `ConnectionStrings__DefaultConnection`.

## Identifier Strategy

- Every aggregate exposed by the API uses a UUID `id` (stored as `PublicId`) for responses and routing.
- Internal tables continue to use integer primary keys for relational integrity; repositories translate between the two.
- Clients should treat the UUID as the canonical identifier and never rely on database integer ids.

## Quick Start

### Using Makefile (Recommended)

The project includes a comprehensive Makefile for common operations:

```bash
# Complete development setup (restore, build, start Docker, apply migrations)
make dev-setup

# Run the API locally
make run

# Or run with hot reload
make run-watch
```

### Manual Setup

```bash
# Restore packages
dotnet restore ExpenseTracker.sln

# Build the solution
dotnet build ExpenseTracker.sln

# Apply database migrations (see Database Migrations section)
make migrate-update

# Seed the database (optional)
make seed

# Run the API
dotnet run --project src/ExpenseTrackerApi/ExpenseTrackerApi.csproj
```

API will listen on `https://localhost:5001` / `http://localhost:5000` by default. Swagger UI is enabled in development at `/swagger`.

### Available Makefile Commands

Run `make help` to see all available commands. Key commands:

**Build & Run:**

- `make restore` - Restore NuGet packages
- `make build` - Build the solution
- `make run` - Run the API locally
- `make run-watch` - Run with hot reload

**Database:**

- `make migrate NAME=migration-name` - Create a new migration
- `make migrate-update` - Apply pending migrations to the database
- `make migrate-list` - List all migrations
- `make migrate-remove` - Remove the last migration
- `make migrate-reset NAME=InitialCreate` - Reset migrations (drops DB and removes all migrations)
- `make seed` - Manually seed the database

**Docker:**

- `make docker-build` - Build Docker image
- `make docker-up` - Start all Docker containers
- `make docker-down` - Stop all Docker containers
- `make docker-restart` - Restart all containers
- `make docker-logs` - View container logs

**Development:**

- `make dev-setup` - Complete development setup
- `make dev-reset` - Reset development environment

## Database Migrations

**Important:** Migrations are **manual** and must be applied explicitly. The API does not automatically apply migrations on startup.

### Creating Migrations

```bash
# Using Makefile (recommended)
make migrate NAME=YourMigrationName

# Or manually
dotnet ef migrations add YourMigrationName \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTrackerApi \
  --output-dir Data/Migrations
```

### Applying Migrations

```bash
# Using Makefile (recommended)
make migrate-update

# Or manually
dotnet ef database update \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTrackerApi
```

### Other Migration Commands

```bash
# List all migrations
make migrate-list

# Remove the last migration
make migrate-remove

# Reset migrations (drops database and removes all migrations)
make migrate-reset NAME=InitialCreate
```

**Note:** Always commit generated migration files so other environments can bootstrap the schema.

## Database Seeding

**Important:** Database seeding is **manual** and must be run explicitly. The API does not automatically seed data on startup.

### Seeding the Database

```bash
# Using Makefile (recommended)
make seed

# Or manually
dotnet run --project src/ExpenseTrackerApi -- --seed
```

This will populate the database with initial data (users, categories, etc.) as defined in the `DataSeeder` class.

## Docker Deployment

The repository includes a multi-stage `Dockerfile` and `docker-compose.yml`:

### Using Makefile

```bash
# Build Docker image
make docker-build

# Start all containers
make docker-up

# Stop all containers
make docker-down

# View logs
make docker-logs

# View API logs only
make docker-logs-api

# View database logs only
make docker-logs-db
```

### Manual Docker Commands

```bash
# Start services
docker compose up --build

# Start in detached mode
docker compose up -d

# Stop services
docker compose down

# View logs
docker compose logs -f
```

### Services

- `api` — ExpenseTracker API (exposes port `8080` by default)
- `sqlserver` — SQL Server 2022 container with persistent volume

**Important:** After starting Docker containers, you must manually apply migrations:

```bash
# Apply migrations to the Docker database
make migrate-update
```

The connection string in `docker-compose.yml` is configured to connect to the SQL Server container.

**Note:** Migrations are not automatically applied when using Docker. You must run `make migrate-update` after starting the containers.
