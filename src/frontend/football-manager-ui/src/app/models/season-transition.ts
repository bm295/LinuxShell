import { NextFixture } from './club-dashboard';

export interface StartNextSeasonResult {
  seasonId: string;
  seasonName: string;
  nextFixture: NextFixture | null;
  summary: string;
}
