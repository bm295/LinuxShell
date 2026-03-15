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
      items.push({ label: 'Club Dashboard', path: `/dashboard/${activeGame.gameId}` });
      items.push({ label: 'Squad', path: `/squad/${activeGame.gameId}` });
    }

    return items;
  });
}
