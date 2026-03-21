import { Component, computed, ElementRef, HostListener, inject, signal, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { appPaths } from './core/routing/app-paths';
import { ActiveGameService } from './core/services/active-game.service';
import { GameMenuAction, GameMenuService } from './core/services/game-menu.service';

type AppMenuKey = 'game' | 'club' | 'league';

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
  @ViewChild('clubMenuContainer') private clubMenuContainer?: ElementRef<HTMLElement>;
  @ViewChild('leagueMenuContainer') private leagueMenuContainer?: ElementRef<HTMLElement>;
  readonly activeMenu = signal<AppMenuKey | null>(null);

  readonly clubNavigation = computed(() => {
    const activeGame = this.activeGameService.activeGame();
    return activeGame
      ? [
          { label: 'Club Dashboard', path: appPaths.dashboard },
          { label: 'Squad', path: appPaths.squad },
          { label: 'Line Up', path: appPaths.lineup },
          { label: 'Academy', path: appPaths.academy }
        ]
      : [];
  });

  readonly navigation = computed(() => {
    const items: Array<{ label: string; path: string }> = [];
    const activeGame = this.activeGameService.activeGame();

    if (activeGame) {
      items.push({ label: 'Transfer Market', path: appPaths.transferMarket });
      items.push({ label: 'Finances', path: appPaths.finances });
    }

    return items;
  });
  readonly leagueNavigation = computed(() => {
    const activeGame = this.activeGameService.activeGame();
    return activeGame
      ? [
          { label: 'Match Center', path: appPaths.matchCenter },
          { label: 'League Table', path: appPaths.leagueTable },
          { label: 'Fixtures', path: appPaths.fixtures }
        ]
      : [];
  });

  readonly hasActiveGame = computed(() => this.activeGameService.activeGame() !== null);

  isMenuOpen(menu: AppMenuKey): boolean {
    return this.activeMenu() === menu;
  }

  openMenu(menu: AppMenuKey, event?: Event): void {
    event?.stopPropagation();
    this.activeMenu.set(menu);
  }

  closeMenu(menu?: AppMenuKey): void {
    if (menu && this.activeMenu() !== menu) {
      return;
    }

    this.activeMenu.set(null);
  }

  handleMenuFocusOut(event: FocusEvent, menu: AppMenuKey): void {
    const nextFocusedElement = event.relatedTarget;
    const menuContainer = this.getMenuContainer(menu);

    if (nextFocusedElement && menuContainer?.contains(nextFocusedElement as Node)) {
      return;
    }

    this.closeMenu(menu);
  }

  openNewGame(): void {
    this.closeMenu();
    void this.router.navigateByUrl(appPaths.newGame);
  }

  navigateTo(path: string): void {
    this.closeMenu();
    void this.router.navigateByUrl(path);
  }

  triggerGameMenuAction(action: GameMenuAction): void {
    if (action === 'save' && !this.hasActiveGame()) {
      return;
    }

    this.gameMenuService.requestAction(action);
    this.closeMenu();
    void this.router.navigateByUrl(appPaths.home);
  }

  @HostListener('document:click', ['$event'])
  handleDocumentClick(event: MouseEvent): void {
    if (!this.activeMenu()) {
      return;
    }

    const target = event.target as Node;
    if (
      !this.gameMenuContainer?.nativeElement.contains(target) &&
      !this.clubMenuContainer?.nativeElement.contains(target) &&
      !this.leagueMenuContainer?.nativeElement.contains(target)
    ) {
      this.closeMenu();
    }
  }

  @HostListener('document:keydown.escape')
  handleEscape(): void {
    this.closeMenu();
  }

  private getMenuContainer(menu: AppMenuKey): HTMLElement | undefined {
    switch (menu) {
      case 'game':
        return this.gameMenuContainer?.nativeElement;
      case 'club':
        return this.clubMenuContainer?.nativeElement;
      default:
        return this.leagueMenuContainer?.nativeElement;
    }
  }
}
