# Phase 0 — Project Bootstrap

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

# Phase MVP

After this phase the developer must be able to:

1 Run docker compose

2 Open the UI

3 See a running Angular application

4 Backend health endpoint works

No gameplay yet.