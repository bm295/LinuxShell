import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { ClubOption } from '../../models/club-option';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);

  readonly activeDashboard = signal<ClubDashboard | null>(null);
  readonly clubPreview = signal<ClubOption[]>([]);
  readonly clubPreviewState = signal<'idle' | 'loading' | 'ready' | 'error'>('idle');
  readonly clubPreviewMessage = signal<string | null>(null);
  readonly summaryState = signal<'idle' | 'loading' | 'ready' | 'error'>('idle');
  readonly summaryMessage = signal<string | null>(null);
  readonly loadedGameId = signal<string | null>(null);
  readonly activeGame = this.activeGameService.activeGame;
  readonly dashboardLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? `/dashboard/${activeGame.gameId}` : '/new-game';
  });
  readonly squadLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? `/squad/${activeGame.gameId}` : '/new-game';
  });
  readonly lineupLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? `/lineup/${activeGame.gameId}` : '/new-game';
  });
  readonly featuredPlayerLink = computed(() => {
    const activeGame = this.activeGame();
    const featuredPlayerId = this.activeDashboard()?.featuredPlayer.id;
    return activeGame && featuredPlayerId ? `/player/${activeGame.gameId}/${featuredPlayerId}` : '/new-game';
  });
  readonly nextFixtureLabel = computed(() => {
    const fixture = this.activeDashboard()?.nextFixture;

    if (!fixture) {
      return 'No fixture scheduled';
    }

    return `${fixture.homeClub} vs ${fixture.awayClub}`;
  });
  readonly featuredClubs = computed(() => this.clubPreview().slice(0, 4));
  readonly heroLabel = computed(() => this.activeGame() ? 'Club Command' : 'Career Start');
  readonly heroTitle = computed(() => {
    const currentClub = this.activeDashboard()?.clubName ?? this.activeGame()?.selectedClub;
    return currentClub
      ? `${currentClub} needs your next selection call.`
      : 'Choose your club and begin the climb.';
  });
  readonly heroCopy = computed(() => this.activeGame()
    ? 'The week now runs through the squad list. Balance form, fitness, and mood before the next whistle.'
    : 'A fresh fictional league is ready. Pick the badge, own the pressure, and start your first season as the manager in charge.');
  readonly featuredPlayerStatus = computed(() => this.activeDashboard()?.featuredPlayer.isStarter
    ? 'Driving the current XI'
    : 'Pushing the starters in training');
  readonly lineupPrompt = computed(() => {
    const lineup = this.activeDashboard()?.lineup;

    if (!lineup) {
      return 'Shape the XI before the next fixture arrives.';
    }

    return `${lineup.formationName} is ${lineup.readiness.toLowerCase()}.`;
  });

  constructor() {
    effect(() => {
      const activeGame = this.activeGame();

      if (!activeGame) {
        this.activeDashboard.set(null);
        this.summaryState.set('idle');
        this.summaryMessage.set(null);
        this.loadedGameId.set(null);

        if (this.clubPreviewState() === 'idle') {
          this.loadClubPreview();
        }

        return;
      }

      if (this.loadedGameId() === activeGame.gameId) {
        return;
      }

      this.loadActiveSummary(activeGame.gameId);
    });
  }

  ngOnInit(): void {
    if (!this.activeGame() && this.clubPreviewState() === 'idle') {
      this.loadClubPreview();
    }
  }

  private loadActiveSummary(gameId: string): void {
    this.loadedGameId.set(gameId);
    this.summaryState.set('loading');
    this.summaryMessage.set(null);

    this.api.getClubDashboard(gameId).subscribe({
      next: (dashboard) => {
        this.activeDashboard.set(dashboard);
        this.summaryState.set('ready');
      },
      error: () => {
        this.activeDashboard.set(null);
        this.summaryState.set('error');
        this.summaryMessage.set('This career could not be resumed right now. Start a new campaign to get back to the dugout.');
      }
    });
  }

  private loadClubPreview(): void {
    this.clubPreviewState.set('loading');
    this.clubPreviewMessage.set(null);

    this.api.getAvailableClubs().subscribe({
      next: (clubs) => {
        this.clubPreview.set(clubs);
        this.clubPreviewState.set('ready');
      },
      error: () => {
        this.clubPreview.set([]);
        this.clubPreviewState.set('error');
        this.clubPreviewMessage.set('The league office has not revealed the club list yet. Step into New Game and try again.');
      }
    });
  }
}
