export interface LeagueTableEntry {
  clubId: string;
  clubName: string;
  position: number;
  played: number;
  wins: number;
  draws: number;
  losses: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
}

export interface FixtureSummary {
  id: string;
  homeClub: string;
  awayClub: string;
  roundNumber: number;
  scheduledAt: string;
  isPlayed: boolean;
  homeGoals: number | null;
  awayGoals: number | null;
  playedAt: string | null;
  isManagedClubFixture: boolean;
  isCurrentRound: boolean;
}
