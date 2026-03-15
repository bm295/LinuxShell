export interface SquadPlayer {
  id: string;
  name: string;
  position: string;
  age: number;
  squadNumber: number;
  attack: number;
  defense: number;
  passing: number;
  fitness: number;
  morale: number;
  overallRating: number;
  isCaptain: boolean;
  isStarter: boolean;
  isInjured: boolean;
  injuryMatchesRemaining: number;
}

export interface SquadSummary {
  totalPlayers: number;
  averageRating: number;
  goalkeepers: number;
  defenders: number;
  midfielders: number;
  forwards: number;
  players: SquadPlayer[];
}

export interface PlayerDetail extends SquadPlayer {
  roleStatus: string;
  managerNote: string;
}
