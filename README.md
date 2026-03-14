# Football Manager Bootstrap

Phase 0 converts the repository into a monorepo with:

- Angular 19 frontend
- .NET 10 backend
- PostgreSQL database
- EF Core migrations
- Seeded league, clubs, and players

## Repository Layout

```text
src/
  backend/
    FootballManager.Api
    FootballManager.Application
    FootballManager.Domain
    FootballManager.Infrastructure
  frontend/
    football-manager-ui
tests/
docs/
docker/
```

## Prerequisites

Install these before setting up the repo:

- Git
- .NET 10 SDK
- Node.js 22.x and npm
- Docker Desktop

If you want to run without Docker, also install:

- PostgreSQL 17 or another local PostgreSQL instance

## Setup Step By Step

Follow these steps once after cloning the repository.

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd LinuxShell
```

### 2. Restore local .NET tools

This restores `dotnet-ef` from the tool manifest.

```bash
dotnet tool restore
```

### 3. Restore backend dependencies

```bash
dotnet restore FootballManager.slnx
```

### 4. Install frontend dependencies

```bash
cd src/frontend/football-manager-ui
npm install
cd ../../..
```

### 5. Optional verification after setup

```bash
dotnet build FootballManager.slnx
cd src/frontend/football-manager-ui
npm run build
cd ../../..
```

At this point the repository is installed and ready to run.

## Run Step By Step After Installing

Use either Docker or local development.

### Option A: Run with Docker

This is the easiest way to run the full stack.

#### 1. Start Docker Desktop

Make sure Docker Desktop is running before continuing.

#### 2. Start all services

From the repository root:

```bash
docker compose up --build
```

#### 3. Open the application

- Frontend: http://localhost:4200
- Backend health: http://localhost:8080/api/health
- Bootstrap summary: http://localhost:8080/api/bootstrap/summary

#### 4. Stop the stack

Press `Ctrl + C` in the terminal, then run:

```bash
docker compose down
```

### Option B: Run locally without Docker

Use this if you want to run backend and frontend directly on your machine.

#### 1. Start PostgreSQL

Create or use a database with these settings:

- Database: `football_manager`
- User: `postgres`
- Password: `postgres`
- Port: `5432`

The default backend connection string expects exactly those values.

#### 2. Start the backend

From the repository root:

```bash
dotnet run --project src/backend/FootballManager.Api
```

The API applies migrations and seeds data automatically on startup.

#### 3. Start the frontend

Open a second terminal and run:

```bash
cd src/frontend/football-manager-ui
npm start
```

The Angular dev server proxies `/api` to `http://localhost:8080`.

#### 4. Open the application

- Frontend: http://localhost:4200
- Backend health: http://localhost:8080/api/health
- Bootstrap summary: http://localhost:8080/api/bootstrap/summary

## Seed Data

Startup seeding creates:

- 1 league
- 8 clubs
- 20 players per club

## Tests

Run the backend build and tests:

```bash
dotnet test FootballManager.slnx
```

Build the frontend:

```bash
cd src/frontend/football-manager-ui
npm run build
```

## Migrations

The repository includes a local EF tool manifest:

```bash
dotnet tool restore
dotnet ef migrations list --project src/backend/FootballManager.Infrastructure --startup-project src/backend/FootballManager.Api
```

## Architecture

The backend follows a lightweight hexagonal structure:

- `FootballManager.Domain`: entities and domain rules
- `FootballManager.Application`: contracts and service interfaces
- `FootballManager.Infrastructure`: EF Core, persistence, seeders, implementations
- `FootballManager.Api`: REST controllers and composition root
