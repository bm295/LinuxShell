# Phase 2 - Squad Management

## Goal

Allow the user to manage players and lineup.

---

# Backend Tasks

Add entities:

Lineup  
Formation

Player fields must include:

Attack  
Defense  
Passing  
Fitness  
Morale

---

Create APIs:

GET /api/squad

Return list of players.

GET /api/player/{id}

Return player details.

POST /api/lineup

Update starting lineup.

---

# Frontend Tasks

Create pages:

Squad  
Player Detail  
Lineup Editor

Features:

- view squad
- view player details
- select starting lineup
- save lineup

---

## Home Page Update

Extend the home page into a club command center with a stronger squad-management identity.

Layout changes:

- add `Squad` to the top menu when a game is active
- add a squad summary card showing squad size, average rating, and lineup readiness
- add a featured player or lineup spotlight area
- add a quick action linking to the lineup editor

Interaction flow:

1 User opens `Home`
2 User sees squad-focused content that reinforces the feeling of managing a living club roster
3 User selects `Squad` or a squad summary card
4 User reviews players, opens player details, or edits the lineup
5 After saving the lineup, the home page reflects the updated lineup state

Presentation rules:

- the home page should feel like a football manager's headquarters, not a dashboard of instructions
- surface player readiness, selection tension, and lineup decisions in game-like language
- avoid technical status messaging or explanatory setup copy

The home page should now act as a playable-feeling shortcut into squad decisions, not only game creation.

---

# Phase MVP

Player can:

- view squad
- inspect players
- set starting lineup
