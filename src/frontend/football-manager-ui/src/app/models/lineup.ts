import { SquadPlayer } from './squad';

export interface Formation {
  id: string;
  name: string;
  defenders: number;
  midfielders: number;
  forwards: number;
}

export interface Lineup {
  formationId: string;
  formationName: string;
  starterCount: number;
  requiredStarters: number;
  averageRating: number;
  averageFitness: number;
  averageMorale: number;
  readiness: string;
  starterPlayerIds: string[];
}

export interface LineupEditor {
  clubName: string;
  lineup: Lineup;
  formations: Formation[];
  players: SquadPlayer[];
}

export interface UpdateLineupRequest {
  formationId: string;
  playerIds: string[];
}
