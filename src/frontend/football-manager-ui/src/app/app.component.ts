import { Component, computed, ElementRef, HostListener, inject, signal, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { ActiveGameService } from './core/services/active-game.service';
import { GameMenuAction, GameMenuService } from './core/services/game-menu.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private readonly activeGameService = inject(ActiveGameService);
  private readonly gameMenuService = inject(GameMenuService);
  private readonly router = inject(Router);
  @ViewChild('gameMenuContainer') private gameMenuContainer?: ElementRef<HTMLElement>;
  readonly isGameMenuOpen = signal(false);

  readonly navigation = computed(() => {
    const items: Array<{ label: string; path: string }> = [];
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

  readonly hasActiveGame = computed(() => this.activeGameService.activeGame() !== null);

  openGameMenu(event?: Event): void {
    event?.stopPropagation();
    this.isGameMenuOpen.set(true);
  }

  closeGameMenu(): void {
    this.isGameMenuOpen.set(false);
  }

  handleGameMenuFocusOut(event: FocusEvent): void {
    const nextFocusedElement = event.relatedTarget;

    if (nextFocusedElement && this.gameMenuContainer?.nativeElement.contains(nextFocusedElement as Node)) {
      return;
    }

    this.closeGameMenu();
  }

  openNewGame(): void {
    this.closeGameMenu();
    void this.router.navigateByUrl('/new-game');
  }

  triggerGameMenuAction(action: GameMenuAction): void {
    if (action === 'save' && !this.hasActiveGame()) {
      return;
    }

    this.gameMenuService.requestAction(action);
    this.closeGameMenu();
    void this.router.navigateByUrl('/');
  }

  @HostListener('document:click', ['$event'])
  handleDocumentClick(event: MouseEvent): void {
    if (!this.isGameMenuOpen()) {
      return;
    }

    if (!this.gameMenuContainer?.nativeElement.contains(event.target as Node)) {
      this.closeGameMenu();
    }
  }

  @HostListener('document:keydown.escape')
  handleEscape(): void {
    this.closeGameMenu();
  }
}
