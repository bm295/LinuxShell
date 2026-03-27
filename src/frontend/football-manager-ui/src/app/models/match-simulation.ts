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

export interface MatchMvp {
  playerId: string;
  playerName: string;
  clubName: string;
  position: string;
  squadNumber: number;
  overallRating: number;
  mvpAwards: number;
}

export interface PlayerDevelopmentChange {
  playerId: string;
  name: string;
  position: string;
  age: number;
  squadNumber: number;
  isCaptain: boolean;
  playedMatch: boolean;
  overallRating: number;
  overallDelta: number;
  attack: number;
  attackDelta: number;
  defense: number;
  defenseDelta: number;
  passing: number;
  passingDelta: number;
  fitness: number;
  fitnessDelta: number;
  morale: number;
  moraleDelta: number;
}

export interface AcademyDevelopmentChange {
  playerId: string;
  name: string;
  position: string;
  age: number;
  trainingFocus: string;
  overallRating: number;
  overallDelta: number;
  attack: number;
  attackDelta: number;
  defense: number;
  defenseDelta: number;
  passing: number;
  passingDelta: number;
  fitness: number;
  fitnessDelta: number;
  morale: number;
  moraleDelta: number;
  developmentProgress: number;
  developmentProgressDelta: number;
}

export interface SimulatedMatchResult {
  homeTeam: string;
  awayTeam: string;
  score: MatchScore;
  matchEvents: MatchEvent[];
  matchMvp: MatchMvp;
  seniorPlayerDevelopment: PlayerDevelopmentChange[];
  academyDevelopment: AcademyDevelopmentChange[];
  clubStanding: LeagueTableEntry;
  nextFixture: NextFixture | null;
  summary: string;
}
