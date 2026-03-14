# Phase 5 - Transfer Market

## Goal

Add player transfers.

---

# Backend Tasks

Entity:

Transfer

Fields:

PlayerId  
FromClub  
ToClub  
Fee

---

APIs:

GET /api/transfer/market

POST /api/transfer/buy

POST /api/transfer/sell

Update:

club budget  
squad lists

---

# Frontend Tasks

Create page:

Transfer Market

Features:

- browse players
- buy player
- sell player

---

## Home Page Update

Extend the home page to surface transfer drama and squad-building opportunities.

Layout changes:

- add `Transfer Market` to the top menu
- add a transfer summary card showing available budget and active market opportunities
- add a recent transfer activity panel for bought and sold players
- add a market spotlight area for a notable target, sale opportunity, or rumor
- add a quick action linking to the transfer market

Interaction flow:

1 User opens `Home`
2 User sees transfer activity framed as an opportunity to improve the club
3 User selects the transfer market from the menu or summary card
4 User buys or sells players
5 Returning to `Home` updates squad summary, budget, and recent transfer activity

Presentation rules:

- the home page should make transfer decisions feel exciting and consequential
- use game-like language such as targets, deals, arrivals, departures, and market movement
- do not show health checks, setup instructions, or technical guidance

The home page should help the player decide whether squad changes are needed before the next match while reinforcing transfer-market excitement.

---

# Phase MVP

Player can:

buy and sell players.
