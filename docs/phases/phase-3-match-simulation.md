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
SeniorPlayerDevelopment  
AcademyDevelopment

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
- after simulation, open a post-match pop-up
- display a result banner in the first tab
- display match events under the banner in the first tab
- display a senior player development tab
- display an academy development tab

Post-match pop-up rules:

- the pop-up opens immediately after the match simulation completes
- the pop-up contains exactly 3 tabs
- tab 1 is the match review tab with a banner panel at the top and match events listed below it
- tab 2 is the player development tab showing how each senior squad player's stats increased or decreased after the match
- tab 3 is the academy development tab showing which youth players improved after the match
- the player should be able to review all 3 tabs before closing the pop-up

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
5 After simulation, a post-match pop-up opens with tabs for match review, senior player development, and academy development
6 After the pop-up is closed, the home page updates recent result, morale, and fixture status summaries

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
3 review match events in a post-match pop-up
4 review senior squad development changes after the match
5 review academy development changes after the match
6 see standings update
