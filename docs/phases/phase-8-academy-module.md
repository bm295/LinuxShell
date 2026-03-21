# Phase 8 - Academy Module

## Goal

Add academy management and youth promotion.

---

Backend

Add:

AcademyPlayer entity  
academy development logic  
promotion flow into the senior squad

APIs:

GET /api/academy

POST /api/academy/promote

---

Frontend

Create page:

Academy

Show:

academy players  
training status  
potential  
promotion action

Post-match academy review:

- academy development should also surface in the post-match pop-up after match simulation
- the academy tab in that pop-up should show which youth players improved and which stats increased

---

## Home Page Update

Extend the home page with long-term club building through youth development.

Layout changes:

- add `Academy` inside `Club Menu` in the top menu
- add an academy spotlight card for the top youth prospect or most promotion-ready player
- add a youth development summary showing academy depth and current promotion pressure
- add a quick link to the academy page
- connect academy status to the wider squad story so the player can decide between promotion and transfers

Interaction flow:

1 User opens `Home`
2 User sees academy progress as part of the club's ongoing long-term project
3 User reviews the leading youth prospect or promotion candidate
4 User navigates to `Academy`
5 User promotes a youth player to the senior squad
6 Returning to `Home` updates squad and academy summaries

The broader matchday flow should also let the user review academy growth immediately after each simulated match through the academy tab in the post-match pop-up.

Presentation rules:

- the home page should frame youth development as club-building ambition, not as a back-office tool
- academy players should feel like future stories, breakthroughs, and squad decisions
- avoid technical copy or admin-style training language as the main presentation

The home page should now support both short-term match decisions and long-term talent development from the same career hub.

---

# Phase MVP

Player can:

view academy players  
promote an academy player to the senior squad  
review academy stat growth after a match
