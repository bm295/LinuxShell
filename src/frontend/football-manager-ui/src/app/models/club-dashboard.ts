export interface ClubDashboard {
  clubName: string;
  budget: number;
  leaguePosition: number;
  points: number;
  nextFixture: NextFixture | null;
  squadSummary: SquadSummary;
}

export interface NextFixture {
  homeClub: string;
  awayClub: string;
  scheduledAt: string;
  roundNumber: number;
}

export interface SquadSummary {
  totalPlayers: number;
  goalkeepers: number;
  defenders: number;
  midfielders: number;
  forwards: number;
  players: SquadPlayer[];
}

export interface SquadPlayer {
  name: string;
  position: string;
  squadNumber: number;
}
