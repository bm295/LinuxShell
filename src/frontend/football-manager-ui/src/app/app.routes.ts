import { Routes } from '@angular/router';

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
    path: '**',
    redirectTo: ''
  }
];
