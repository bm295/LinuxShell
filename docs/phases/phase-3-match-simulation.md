# Phase 3 — Match Simulation

## Goal

Allow simulation of matches.

---

# Backend Tasks

Create service:

MatchSimulationService

Algorithm inputs:

TeamStrength  
Fitness  
Morale  
Tactics  
RandomFactor

Outputs:

Score  
Events

---

Create API:

POST /api/match/simulate-next

Return:

HomeTeam  
AwayTeam  
Score  
MatchEvents

---

Update:

LeagueTable  
PlayerFitness  
Morale  
Injuries

---

# Frontend Tasks

Create page:

Match Center

Features:

- view next match
- simulate match
- display result
- display events

---

# Phase MVP

Player can:

1 simulate match
2 see score
3 see standings update