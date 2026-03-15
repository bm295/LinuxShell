import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { ActiveGameService } from './core/services/active-game.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private readonly activeGameService = inject(ActiveGameService);

  readonly navigation = computed(() => {
    const items = [
      { label: 'Home', path: '/' },
      { label: 'New Game', path: '/new-game' }
    ];
    const activeGame = this.activeGameService.activeGame();

    if (activeGame) {
      items.push({ label: 'Match Center', path: `/match-center/${activeGame.gameId}` });
      items.push({ label: 'Academy', path: `/academy/${activeGame.gameId}` });
      items.push({ label: 'League Table', path: `/league-table/${activeGame.gameId}` });
      items.push({ label: 'Fixtures', path: `/fixtures/${activeGame.gameId}` });
      items.push({ label: 'Transfer Market', path: `/transfer-market/${activeGame.gameId}` });
      items.push({ label: 'Finances', path: `/finances/${activeGame.gameId}` });
      items.push({ label: 'Club Dashboard', path: `/dashboard/${activeGame.gameId}` });
      items.push({ label: 'Squad', path: `/squad/${activeGame.gameId}` });
      items.push({ label: 'Lineup', path: `/lineup/${activeGame.gameId}` });
    }

    return items;
  });
}
