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
SeasonMvpAwards

---

API

GET /api/league/table

GET /api/fixtures

GET /api/league/top-players

---

# Frontend Tasks

Create pages:

League Table  
Fixtures
Top Players

Show:

league standings  
results history
top 10 current-season players by MVP awards

League navigation rules:

- add `Top Players` under `League Menu`
- the `Top Players` page must show players ordered by season MVP awards in descending order

---

## Home Page Update

Extend the home page with league drama and season progression.

Layout changes:

- group `League Table` and `Fixtures` under `League Menu` in the top menu
- add `Top Players` to the league navigation
- add a compact league table snapshot showing the managed club and nearby positions
- add a season progress panel showing played matches and remaining fixtures
- add a rivalry or title-race panel highlighting nearby competitors
- add quick links to full standings, fixture history, and top players

Interaction flow:

1 User opens `Home`
2 User immediately sees where the club stands in the season story
3 User reviews season progress and current league position
4 User opens `League Table`, `Fixtures`, or `Top Players` from summary cards or the `League Menu`
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
view the season MVP leaderboard through `Top Players`.
