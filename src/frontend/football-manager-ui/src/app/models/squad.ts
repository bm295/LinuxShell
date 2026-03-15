export interface SquadPlayer {
  id: string;
  name: string;
  position: string;
  squadNumber: number;
  attack: number;
  defense: number;
  passing: number;
  fitness: number;
  morale: number;
  overallRating: number;
  isStarter: boolean;
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
