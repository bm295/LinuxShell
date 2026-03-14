import { Routes } from '@angular/router';

import { ClubDashboardComponent } from './features/club-dashboard/club-dashboard.component';
import { HomeComponent } from './features/home/home.component';
import { NewGameComponent } from './features/new-game/new-game.component';

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
    path: '**',
    redirectTo: ''
  }
];
