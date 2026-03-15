# Phase 7 - Save and Load Game

## Goal

Persist game progress.

---

Backend

Add:

GameSave entity.

APIs:

POST /api/game/save

GET /api/game/load

DELETE /api/game/save

---

Frontend

Add buttons:

Save Game  
Load Game

---

## Home Page Update

Finalize the home page as the long-term career hub for returning players.

Layout changes:

- keep the full navigation from earlier phases
- add `Save Game` and `Load Game` actions in the header or hero area
- open recent saves as a pop-up or modal only when `Load Game` is selected
- inside the pop-up, show save name, club, season, and last updated time
- allow each saved game to be loaded or deleted from the pop-up
- add a career spotlight area showing the currently selected save as the player's ongoing story
- add a continue card for the currently loaded save
- show clear success or failure feedback after `Save Game`

Interaction flow:

1 User opens `Home`
2 User sees the current career presented as the main storyline
3 User can save the current game directly from the home page
4 User selects `Load Game` and the save pop-up opens
5 User loads or deletes a save from the pop-up
6 After loading, the home page refreshes all summary cards to the selected save
7 User selects `Continue` to return to the dashboard or most relevant game area

Presentation rules:

- the home page should feel like returning to an ongoing career, not reopening a software tool
- saves should be framed as club journeys, seasons, and milestones
- the load pop-up should feel like browsing active club stories, not a technical file picker
- do not show service health, bootstrap guidance, or technical instructions

The home page should now support both first-time entry and long-term return-to-save usage while preserving a strong sense of career progression.

---

# Phase MVP

Player can:

save progress  
reload game
