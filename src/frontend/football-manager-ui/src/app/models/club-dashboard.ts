import { Lineup } from './lineup';
import { SquadSummary } from './squad';

export interface ClubDashboard {
  clubName: string;
  seasonName: string;
  competitionName: string;
  budget: number;
  leaguePosition: number;
  points: number;
  nextFixture: NextFixture | null;
  recentResult: RecentResult | null;
  momentumNote: string;
  squadSummary: SquadSummary;
  lineup: Lineup;
  featuredPlayer: FeaturedPlayer;
}

export interface NextFixture {
  homeClub: string;
  awayClub: string;
  scheduledAt: string;
  roundNumber: number;
}

export interface RecentResult {
  homeClub: string;
  awayClub: string;
  homeGoals: number;
  awayGoals: number;
  playedAt: string;
  roundNumber: number;
}

export interface FeaturedPlayer {
  id: string;
  name: string;
  position: string;
  squadNumber: number;
  overallRating: number;
  fitness: number;
  morale: number;
  isStarter: boolean;
  isInjured: boolean;
  injuryMatchesRemaining: number;
  spotlight: string;
}
