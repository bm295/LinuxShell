# Phase 3 - Match Simulation

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

## Home Page Update

Extend the home page to create matchday anticipation.

Layout changes:

- add `Match Center` to the top menu
- add a next fixture card with opponent, competition, and match readiness
- add a bold primary quick action that routes to match simulation
- add a recent result panel after a match is completed
- add a momentum area showing morale, form, or matchday narrative

Interaction flow:

1 User opens `Home`
2 User immediately feels that the next meaningful event is the upcoming match
3 User sees the next fixture and selects the match action
4 User is routed to `Match Center`
5 After simulation, the home page updates recent result, morale, and fixture status summaries

Presentation rules:

- the home page should feel like a pre-match hub with tension, momentum, and stakes
- use game-facing language around fixtures, form, and results
- do not show service health, setup guidance, or developer-oriented messaging

The home page should make it obvious and emotionally clear that the next meaningful action is to play the next match.

---

# Phase MVP

Player can:

1 simulate match
2 see score
3 see standings update
