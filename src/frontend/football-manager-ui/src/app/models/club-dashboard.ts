import { Lineup } from './lineup';
import { SquadSummary } from './squad';

export interface ClubDashboard {
  clubName: string;
  seasonName: string;
  budget: number;
  leaguePosition: number;
  points: number;
  nextFixture: NextFixture | null;
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

export interface FeaturedPlayer {
  id: string;
  name: string;
  position: string;
  squadNumber: number;
  overallRating: number;
  fitness: number;
  morale: number;
  isStarter: boolean;
  spotlight: string;
}
