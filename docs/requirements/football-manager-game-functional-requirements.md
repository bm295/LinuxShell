# Football Manager Web Game
# Functional Requirements

## 1. Product Overview

The system is a browser-based football club management game.

The user acts as the manager of a fictional football club and is responsible for managing squad selection, tactics, transfers, finances, and match decisions during a league season.

The game is turn-based and focused on decision-making and simulation rather than real-time gameplay.

The system must provide a simple but complete management gameplay loop.

---

# 2. Target Users

- Single-player only
- Casual players who enjoy sports management simulations
- No multiplayer features required

---

# 3. Core Gameplay Loop

The gameplay loop must follow these steps:

1. Player reviews squad and team status
2. Player sets lineup and tactics
3. Player simulates the next match
4. System updates match results and league standings
5. System updates player conditions (fitness, morale, injuries)
6. Player continues to next match
7. Season continues until all fixtures are completed

---

# 4. Game Start

The game must allow the user to:

- Start a new game
- Choose a club from a fictional league
- Load an existing saved game

When starting a new game, the system must initialize:

- League teams
- Club squads
- Club finances
- Fixture list for the season
- League standings

---

# 5. Club Dashboard

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

---

# 6. Squad Management

The user must be able to:

- View all players in the club
- Inspect player details
- Set starting lineup
- Assign substitute players
- Rest players

Each player must include:

- Name
- Age
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

# 7. Tactical Management

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

# 8. Match Simulation

Users must be able to simulate the next scheduled match.

Match simulation should produce:

- Final score
- Basic match events
- Simple match statistics

Simulation must consider:

- Team strength
- Player fitness
- Player morale
- Tactical setup
- Randomness

After simulation, the system must update:

- League standings
- Player fitness
- Player morale
- Injury status
- Club finances

---

# 9. League System

The game must include:

- One fictional league
- 8–12 clubs
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

---

# 10. Transfers

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

# 11. Club Finances

The game must track:

- Club budget
- Transfer expenses
- Transfer income
- Player wages
- Match income

The system must display a financial overview.

---

# 12. Season Progression

The season must progress fixture by fixture.

At the end of the season:

The system must display:

- Final league table
- Club performance summary

The user must be able to:

- Start a new season
- Continue the game with the current club

---

# 13. Save / Load Game

The game must allow:

- Saving current progress
- Loading saved games

Game state must include:

- Club squad
- Fixtures played
- League standings
- Club finances
- Player conditions