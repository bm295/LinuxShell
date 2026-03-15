import { NextFixture } from './club-dashboard';
import { LeagueTableEntry } from './league';

export interface MatchScore {
  homeGoals: number;
  awayGoals: number;
}

export interface MatchEvent {
  minute: number;
  type: string;
  description: string;
}

export interface SimulatedMatchResult {
  homeTeam: string;
  awayTeam: string;
  score: MatchScore;
  matchEvents: MatchEvent[];
  clubStanding: LeagueTableEntry;
  nextFixture: NextFixture | null;
  summary: string;
}
