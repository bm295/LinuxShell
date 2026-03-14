# Phase 1 - New Game and Club Dashboard

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

## Home Page Update

Upgrade the home page from a bootstrap landing page into the first game-facing screen.

Layout:

- header with `Home`, `New Game`, and `Club Dashboard` when an active game exists
- hero section that feels like the start of a manager career, with a strong `Start New Game` action
- current save summary area that appears after a game is created
- club-themed cards for selected club, season, and next fixture

Presentation rules:

- from this phase onward, the home page should feel like a football management game screen
- do not present backend health, setup steps, or technical instructions on the home page
- copy should focus on club identity, season progression, and the fantasy of starting a manager journey

Interaction flow:

1 User opens `Home`
2 If no save exists, the page highlights the fantasy of choosing a club and beginning a career
3 User selects `Start New Game`
4 User chooses a club on the `New Game` page
5 System creates the game and redirects to `Club Dashboard`
6 Returning to `Home` shows the active club summary and a `Continue` path back to the dashboard

If no save exists, the home page should still feel like a game menu, not a product or service landing page.

---

# Phase MVP

Player can:

1 Start new game
2 Select club
3 See dashboard
4 See squad overview
