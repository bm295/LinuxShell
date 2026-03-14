# Football Manager Web Game
# Technical Requirements

## 1. Repository

The entire system must be implemented in a single monorepo.

The repository must include:

- Backend source code
- Frontend source code
- Database setup
- Documentation
- Tests

---

# 2. Technology Stack

Backend
- Language: C# 14
- Framework: .NET 10
- API: ASP.NET Core Web API

Frontend
- Framework: Angular 19

Database
- PostgreSQL

Containerization
- Docker
- Docker Compose for local environment

---

# 3. Backend Architecture

The backend must follow Hexagonal Architecture.

Layers:

Domain  
Application  
Infrastructure  
API

Responsibilities:

Domain
- Core entities
- Value objects
- Domain rules

Application
- Use cases
- Application services
- DTOs
- Ports/interfaces

Infrastructure
- Database persistence
- Repository implementations
- External integrations

API
- REST endpoints
- Request validation
- Dependency injection

---

# 4. Domain Model

Core entities must include:

Club  
Player  
Match  
Fixture  
Season  
LeagueTableEntry  
Transfer  
GameSave  
Tactic  
Formation  
Finance  
Injury

Relationships must follow domain-driven design principles.

---

# 5. Database

Database must use PostgreSQL.

Requirements:

- Relational schema
- Foreign key relationships
- Migrations supported
- Seed data generation

Seed data must include:

- One league
- 8–12 clubs
- Full squads for each club
- Initial budgets
- Generated fixtures

---

# 6. API Design

The system must expose REST APIs.

Minimum endpoints:

Game
- Create new game
- Load game
- Save game

Club
- Get club dashboard
- Get squad
- Update lineup
- Update tactics

Match
- Get fixtures
- Simulate match
- Get results

League
- Get standings
- Get league table

Transfer
- List transfer players
- Buy player
- Sell player

Finance
- Get financial summary

---

# 7. Frontend Requirements

Frontend must use Angular 19.

The UI must include these screens:

New Game / Load Game  
Club Dashboard  
Squad Management  
Player Detail  
Tactics  
Fixtures and Results  
League Table  
Transfer Market  
Finance Overview

The UI should:

- Use clean layout
- Display data clearly
- Provide feedback after actions

---

# 8. Persistence

All game state must be stored in PostgreSQL.

Game state includes:

- Players
- Clubs
- Fixtures
- Matches
- Standings
- Finances
- Game progress

---

# 9. Testing

Backend testing must include:

- Unit tests for domain logic
- Tests for match simulation
- API integration tests where appropriate

Frontend testing may include:

- Component tests
- Critical flow tests

---

# 10. Deployment

The system must support local deployment.

Required components:

- Docker Compose configuration
- PostgreSQL container
- Backend service
- Frontend service

---

# 11. Code Quality

Code must follow these principles:

- SOLID principles
- Clean Architecture
- Meaningful naming
- Avoid unnecessary complexity

---

# 12. Documentation

The repository must include:

README.md

Documentation should cover:

- Project overview
- Setup instructions
- Running the application
- Architecture overview