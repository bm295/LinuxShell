# Phase 4 - League System

## Goal

Add full league standings and fixture progression.

---

# Backend Tasks

Entities:

Fixture  
LeagueTableEntry

Add logic:

generate season schedule.

Track:

Played  
Wins  
Draws  
Losses  
Points

---

API

GET /api/league/table

GET /api/fixtures

---

# Frontend Tasks

Create pages:

League Table  
Fixtures

Show:

league standings  
results history

---

## Home Page Update

Extend the home page with league drama and season progression.

Layout changes:

- add `League Table` and `Fixtures` to the top menu
- add a compact league table snapshot showing the managed club and nearby positions
- add a season progress panel showing played matches and remaining fixtures
- add a rivalry or title-race panel highlighting nearby competitors
- add quick links to full standings and fixture history

Interaction flow:

1 User opens `Home`
2 User immediately sees where the club stands in the season story
3 User reviews season progress and current league position
4 User opens `League Table` or `Fixtures` from summary cards
5 After matches are played, the home page refreshes the standings snapshot and fixture timeline

Presentation rules:

- the home page should feel like the center of an active competition, with pressure from rivals and targets ahead
- highlight league stakes, not instructions about how the application works
- avoid technical or service-oriented language

The home page should now summarize both club state and league state with a competitive, game-like tone.

---

# Phase MVP

Player can:

play matches through a season.
