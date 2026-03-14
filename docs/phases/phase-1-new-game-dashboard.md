# Phase 1 — New Game and Club Dashboard

## Goal

Create the first playable MVP.

The player can:

- start a new game
- choose a club
- view club dashboard

---

# Backend Tasks

## Create Domain Entities

Club  
Player  
Season  
GameSave

---

## Create Use Cases

CreateNewGame

Steps:

1 Create new season
2 Assign clubs
3 Assign squads
4 Generate fixtures
5 Save GameSave

---

## API

POST /api/game/new

Response:

GameId  
SelectedClub  
SeasonId

---

GET /api/club/dashboard

Return:

ClubName  
Budget  
LeaguePosition  
Points  
NextFixture  
SquadSummary

---

# Frontend Tasks

Create pages:

New Game  
Club Dashboard

New Game Page:

- list clubs
- choose club
- start game

After starting game:

redirect to dashboard.

---

# Phase MVP

Player can:

1 Start new game
2 Select club
3 See dashboard
4 See squad overview