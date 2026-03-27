# Football Manager Web Game
# Functional Requirements

## 1. Product Overview

The system is a browser-based football club management game.

The user acts as the manager of a fictional football club and is responsible for managing squad selection, tactics, transfers, finances, academy development, and match decisions during a league season.

The game is turn-based and focused on decision-making and simulation rather than real-time gameplay.

The system must provide a simple but complete management gameplay loop.

---

# 2. Target Users

- Single-player only
- Casual players who enjoy sports management simulations
- No multiplayer features required

---

# 3. Home Page and Navigation

The system must provide a home page that acts as the main entry point before and during gameplay.

The home page must communicate:

- game title and theme
- current game status
- primary actions available to the player
- clear navigation to unlocked management areas

The home page layout should include:

- a top header with game title and primary menu
- a main hero area with the primary call to action
- summary cards for current save information and key club status
- quick links to the main game areas
- a responsive layout that stacks cleanly on smaller screens

The home page presentation after the new game flow is introduced must:

- feel like part of the football management game, not a technical product landing page
- emphasize club identity, season progression, match stakes, and management choices
- avoid using backend health messages, setup instructions, or developer-oriented copy as primary home page content

The top menu must support the following navigation model:

- Home
- Game Menu
  - Save Game
  - Load Game
  - New Game
- Club Menu
  - Club Dashboard
  - Squad
  - Line Up
  - Academy
- League Menu
  - Match Center
  - League Table
  - Fixtures
  - Top Players
- Transfer Market
- Finances

Menu items may be hidden or disabled until the related feature is implemented or until a game save exists.

The home page interaction flow must support:

1. First visit:
   - display product title, short game description, and a primary `Start New Game` action
   - any technical or system status messaging must be secondary and should not dominate the page once gameplay exists
2. New game flow:
   - selecting `Start New Game` opens the club selection flow
   - after club selection and game creation, the player is redirected to the club dashboard
3. Continue flow:
   - when an active save exists, the home page must show a `Continue` action that returns the player to the latest in-progress view or club dashboard
4. Load flow:
   - selecting `Load Game` opens a recent saves pop-up or modal on the home page
   - the pop-up must list available saves with save name, club, season, and last updated time
   - each save entry in the pop-up must support `Load` and `Delete`
   - after a save is loaded, the home page refreshes to show the selected save summary
5. In-game navigation:
   - each summary card or quick action on the home page must deep-link to the relevant game area
   - returning to the home page must preserve the current game context

When a save exists, the home page should summarize:

- managed club
- current season
- league position
- next fixture
- budget
- squad status
- academy status
- latest important actions or results

---

# 4. Core Gameplay Loop

The gameplay loop must follow these steps:

1. Player reviews squad, academy, and team status
2. Player sets lineup and tactics
3. Player reviews transfers, finances, and promotion options
4. Player simulates the next match
5. System updates match results, league standings, and season MVP records
6. System updates player conditions, academy training progress, and club finances
7. Player continues to next match
8. Season continues until all fixtures are completed

---

# 5. Game Start

The game must allow the user to:

- Start a new game
- Choose a club from a fictional league
- Load an existing saved game

When starting a new game, the system must initialize:

- League teams
- Club squads
- Club academy prospects
- Club finances
- Fixture list for the season
- League standings

---

# 6. Club Dashboard

The club dashboard must display:

- Club name
- Current budget
- Wage spending
- Current league position
- Points
- Upcoming fixture
- Recent match results
- Squad overview
- Team overall rating
- Academy prospect summary

---

# 7. Squad Management

The user must be able to:

- View all players in the club
- Inspect player details
- Change a player's position
- Set starting lineup
- Assign substitute players
- Rest players

Promoted academy players must join the senior squad and become available through the same squad management flow as other players.

Each senior squad must always have exactly one captain.

The system should surface post-match senior player development changes through a dedicated Player Development tab in the match results pop-up.

Each player must include:

- Name
- Age
- Jersey number
- Captain status
- Position
- Overall rating
- Attack attribute
- Defense attribute
- Passing attribute
- Stamina
- Morale
- Fitness
- Market value
- Weekly salary
- Contract duration
- Injury status

---

# 8. Tactical Management

The system must allow the player to configure tactics.

Supported formations:

- 4-4-2
- 4-3-3
- 3-5-2

Supported tactical styles:

- Defensive
- Balanced
- Attacking

Tactics should influence match simulation results.

---

# 9. Match Simulation

Users must be able to simulate the next scheduled match.

Match simulation should produce:

- Final score
- Basic match events
- Simple match statistics
- Match MVP

Simulation must consider:

- Team strength
- Player fitness
- Player morale
- Player age profile
- Tactical setup
- Randomness

After simulation, the system must update:

- League standings
- Player fitness
- Player morale
- Player core attributes based on age progression or decline
- Injury status
- Club finances
- Academy training progress
- Current-season MVP award totals for the selected match MVP

The system must determine exactly one MVP for each completed match and show that player in the post-match result flow.

After each match simulation, the system must open a post-match pop-up or modal with exactly 3 tabs:

- Match Review tab:
  - show a banner-style summary panel at the top with the fixture and result
  - show the MVP of the match in the summary area
  - list match events below the banner
- Player Development tab:
  - show each senior squad player's stat changes after the match
  - clearly indicate increases and decreases
- Academy Development tab:
  - show academy or youth player stat increases after the match
  - focus on development progress rather than match events

The post-match pop-up must be the primary place where the player reviews the result and both senior and youth development before continuing.

---

# 10. League System

The game must include:

- One fictional league
- 8-12 clubs
- Home and away fixtures

League standings must track:

- Played matches
- Wins
- Draws
- Losses
- Goals scored
- Goals conceded
- Goal difference
- Points

The league area must also include a `Top Players` view under `League Menu`.

The `Top Players` view must:

- show the top 10 players in the current season
- order players by number of MVP awards in descending order
- display each player's name, club, and season MVP total

---

# 11. Transfers

The game must include a simple transfer system.

The user must be able to:

- View available transfer players
- Buy players
- Sell players

Transfer rules:

- Transfers require sufficient club budget
- Player moves between clubs
- Club budget updates after transfers

---

# 12. Club Finances

The game must track:

- Club budget
- Transfer expenses
- Transfer income
- Player wages
- Match income

The system must display a financial overview.

---

# 13. Season Progression

The season must progress fixture by fixture.

At the end of the season:

The system must display:

- Final league table
- Club performance summary

The user must be able to:

- Start a new season
- Continue the game with the current club

---

# 14. Save / Load Game

The game must allow:

- Saving current progress
- Loading saved games
- Deleting saved games

Save / load interaction rules:

- `Load Game` on the home page should open recent saves as a pop-up rather than a permanently visible panel
- loading and deleting saves should happen from that pop-up flow
- saving from the home page must show a clear success or failure message to the player

Game state must include:

- Club squad
- Fixtures played
- League standings
- Club finances
- Player conditions
- Academy players and development progress

---

# 15. Academy Module

The game must include an academy module for youth development.

The academy module must allow the player to:

- View academy players who are not yet part of the senior squad
- Review each academy player's age, position, current rating, potential, and training status
- Track academy development over time as matchdays or rounds pass
- Promote academy players into the senior squad

Academy rules:

- Each club should have a small academy group or youth intake available during a save
- Academy players should improve gradually through training and development updates
- Promotion should move the player from the academy into the senior squad
- Promoted players should receive a squad number and become selectable in squad and lineup screens
- Promotion may be limited by squad size, club rules, or available registration space if those systems exist

The system should display academy information on both the dedicated academy page and the home page.

The system should also display academy development gains after match simulation through the Academy Development tab in the post-match pop-up.
