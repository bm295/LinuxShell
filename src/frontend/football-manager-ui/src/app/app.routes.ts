import { CanDeactivateFn, Routes } from '@angular/router';

import { ClubDashboardComponent } from './features/club-dashboard/club-dashboard.component';
import { AcademyComponent } from './features/academy/academy.component';
import { HomeComponent } from './features/home/home.component';
import { FixturesComponent } from './features/fixtures/fixtures.component';
import { FinancesComponent } from './features/finances/finances.component';
import { LeagueTableComponent } from './features/league-table/league-table.component';
import { LineupEditorComponent } from './features/lineup-editor/lineup-editor.component';
import { MatchCenterComponent } from './features/match-center/match-center.component';
import { NewGameComponent } from './features/new-game/new-game.component';
import { PlayerDetailComponent } from './features/player-detail/player-detail.component';
import { SquadComponent } from './features/squad/squad.component';
import { TransferMarketComponent } from './features/transfer-market/transfer-market.component';

const blockPendingPlayerDetailNavigation: CanDeactivateFn<PlayerDetailComponent> = (component) =>
  component.canDeactivate();

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent
  },
  {
    path: 'new-game',
    component: NewGameComponent
  },
  {
    path: 'dashboard',
    component: ClubDashboardComponent
  },
  {
    path: 'dashboard/:gameId',
    component: ClubDashboardComponent
  },
  {
    path: 'academy',
    component: AcademyComponent
  },
  {
    path: 'academy/:gameId',
    component: AcademyComponent
  },
  {
    path: 'squad',
    component: SquadComponent
  },
  {
    path: 'squad/:gameId',
    component: SquadComponent
  },
  {
    path: 'player/:playerKey',
    component: PlayerDetailComponent,
    canDeactivate: [blockPendingPlayerDetailNavigation]
  },
  {
    path: 'player/:gameId/:playerId',
    component: PlayerDetailComponent,
    canDeactivate: [blockPendingPlayerDetailNavigation]
  },
  {
    path: 'lineup',
    component: LineupEditorComponent
  },
  {
    path: 'lineup/:gameId',
    component: LineupEditorComponent
  },
  {
    path: 'match-center',
    component: MatchCenterComponent
  },
  {
    path: 'match-center/:gameId',
    component: MatchCenterComponent
  },
  {
    path: 'league-table',
    component: LeagueTableComponent
  },
  {
    path: 'league-table/:gameId',
    component: LeagueTableComponent
  },
  {
    path: 'fixtures',
    component: FixturesComponent
  },
  {
    path: 'fixtures/:gameId',
    component: FixturesComponent
  },
  {
    path: 'transfer-market',
    component: TransferMarketComponent
  },
  {
    path: 'transfer-market/:gameId',
    component: TransferMarketComponent
  },
  {
    path: 'finances',
    component: FinancesComponent
  },
  {
    path: 'finances/:gameId',
    component: FinancesComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
