import { Routes } from '@angular/router';

import { ClubDashboardComponent } from './features/club-dashboard/club-dashboard.component';
import { HomeComponent } from './features/home/home.component';
import { LineupEditorComponent } from './features/lineup-editor/lineup-editor.component';
import { NewGameComponent } from './features/new-game/new-game.component';
import { PlayerDetailComponent } from './features/player-detail/player-detail.component';
import { SquadComponent } from './features/squad/squad.component';

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
    path: 'dashboard/:gameId',
    component: ClubDashboardComponent
  },
  {
    path: 'squad/:gameId',
    component: SquadComponent
  },
  {
    path: 'player/:gameId/:playerId',
    component: PlayerDetailComponent
  },
  {
    path: 'lineup/:gameId',
    component: LineupEditorComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
