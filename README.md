# Football Manager-Style Simulation (Godot + C#)

This repository is now a starting point for a Football Manager-style management game.
The project uses **Godot 4** with **C#** to provide a strong UI toolkit, fast iteration,
robust tooling, and a good fit for data-heavy simulation gameplay.

## Why C# + Godot
- **Management-game friendly:** Data-centric simulation and UI-heavy workflows.
- **Excellent tooling:** Strong typing, IDE support, and debugging.
- **Open-source engine:** Godot is lightweight and flexible for custom tooling.

## Project Structure
- `project.godot`: Godot project configuration.
- `Main.tscn`: Entry scene and UI layout.
- `Scripts/`: Core gameplay and simulation code.
- `Data/`: Placeholder for JSON/YAML databases (players, clubs, leagues).
- `Archived/`: Legacy scripts moved from the old repo.

## Getting Started
1. Install [Godot 4.x (.NET)](https://godotengine.org/download).
2. Open the project in Godot by selecting this repository folder.
3. Run `Main.tscn`.
4. Use the in-game menu to simulate matches, view the club list, reset the season, or exit.

## UI Menu (Current)
- **Mo phong tran dau:** Simulate a match between two clubs.
- **Xem danh sach CLB:** Display the league club list.
- **Tao mua giai moi:** Create a fresh league instance.
- **CLB cua toi:** View and manage your club.
  - **Thong tin CLB:** Show current club name, reputation, and budget.
  - **Tang uy tin (-200k):** Spend budget to raise reputation.
  - **Ky tai tro (+500k):** Increase budget with a sponsor deal.
  - **Doi ten:** Rename your club using the input field.
- **Thoat:** Quit the game.

## Next Steps
- Build a data importer (players/clubs/leagues) from `Data/`.
- Add season scheduling, match simulation, finances, and scouting.
- Build UI screens (squad, tactics, fixtures, transfers).
