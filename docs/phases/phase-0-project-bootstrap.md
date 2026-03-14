# Phase 0 - Project Bootstrap

## Goal

Create the base monorepo structure and minimal runnable application.

After this phase the system must run locally with:

- Angular frontend
- .NET backend
- PostgreSQL database
- Seed data

The system does not need gameplay yet.

---

# Tasks

## 1 Create Monorepo Structure

Create repository structure:

/src
/backend
FootballManager.Api
FootballManager.Application
FootballManager.Domain
FootballManager.Infrastructure

/frontend
football-manager-ui

/tests

/docs

/docker

---

## 2 Backend Setup

Use:

C# 14  
.NET 10  
ASP.NET Core Web API

Add:

- Dependency Injection
- Basic controller
- Health endpoint

Example:

GET /api/health

Response:

{ "status": "ok" }

---

## 3 Database Setup

Use PostgreSQL.

Add:

- EF Core
- Migrations support
- Basic connection configuration

Create simple table:

Game

Fields:

Id  
CreatedAt

---

## 4 Seed Data

Add simple seed process.

Create:

- 1 league
- 8 clubs
- 20 players per club

No gameplay yet.

---

## 5 Frontend Setup

Use Angular 19.

Create pages:

Home  
New Game

The home page should call:

GET /api/health

Display backend status.

---

## 6 Docker

Create docker-compose:

services:

postgres  
backend  
frontend

---

## 7 Home Page Update

The home page in this phase is a minimal landing page.

Layout:

- header with game title
- simple top menu with `Home` and `New Game`
- primary content area with short product description
- backend status card fed by `GET /api/health`

Interaction flow:

1 Open the application
2 Home page requests backend health
3 Status is shown as available or unavailable
4 User can navigate to the `New Game` page, which is a placeholder at this stage

Do not expose gameplay summaries yet because no game session exists in this phase.

---

# Phase MVP

After this phase the developer must be able to:

1 Run docker compose

2 Open the UI

3 See a running Angular application

4 Backend health endpoint works

No gameplay yet.
