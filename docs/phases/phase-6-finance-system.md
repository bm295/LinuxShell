# Phase 6 - Finance System

## Goal

Add financial management.

---

Track:

ClubBudget  
Wages  
TransferSpending  
MatchIncome

---

API

GET /api/finance

Return financial summary.

---

Frontend

Create page:

Finances

Show:

budget  
expenses  
income

---

## Home Page Update

Extend the home page with boardroom pressure and financial consequences.

Layout changes:

- add `Finances` to the top menu
- add finance cards for current budget, wage total, transfer spending, and recent income
- add a short trend or summary area explaining the latest financial changes
- add a board-confidence or club-health panel tied to financial decisions
- add a quick link to the detailed finance page

Interaction flow:

1 User opens `Home`
2 User sees the financial state as part of the club's ongoing story and pressure
3 User reviews the latest financial state
4 User navigates to `Finances` for full details
5 After transfers or matches, the home page refreshes the financial summary cards

Presentation rules:

- the home page should present finances as meaningful management stakes, not as dry accounting instructions
- combine sporting ambition with budget pressure, wages, and recent income changes
- avoid technical copy unrelated to the football-management fantasy

The home page should now support both sporting and financial decision-making at a glance while keeping a game-like atmosphere.

---

# Phase MVP

Player can see financial impact of decisions.
