import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { appPaths, buildPlayerPath } from '../../core/routing/app-paths';
import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { GameMenuService } from '../../core/services/game-menu.service';
import { AcademyPlayer, AcademySummary } from '../../models/academy';
import { ClubDashboard } from '../../models/club-dashboard';
import { ClubOption } from '../../models/club-option';
import { FinanceSummary } from '../../models/finance';
import { GameSaveSummary } from '../../models/game-save';
import { FixtureSummary, LeagueTableEntry } from '../../models/league';
import { TransferMarket } from '../../models/transfer-market';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly gameMenuService = inject(GameMenuService);
  private deleteToastTimeoutId: ReturnType<typeof setTimeout> | null = null;

  readonly activeDashboard = signal<ClubDashboard | null>(null);
  readonly academy = signal<AcademySummary | null>(null);
  readonly leagueTable = signal<LeagueTableEntry[]>([]);
  readonly fixtures = signal<FixtureSummary[]>([]);
  readonly transferMarket = signal<TransferMarket | null>(null);
  readonly financeSummary = signal<FinanceSummary | null>(null);
  readonly saveLibrary = signal<GameSaveSummary[]>([]);
  readonly clubPreview = signal<ClubOption[]>([]);
  readonly clubPreviewState = signal<'idle' | 'loading' | 'ready' | 'error'>('idle');
  readonly clubPreviewMessage = signal<string | null>(null);
  readonly saveState = signal<'idle' | 'loading' | 'ready' | 'error'>('idle');
  readonly saveMessage = signal<string | null>(null);
  readonly saveActionMessage = signal<string | null>(null);
  readonly deleteToastMessage = signal<string | null>(null);
  readonly isSavingGame = signal(false);
  readonly loadingSaveId = signal<string | null>(null);
  readonly deletingSaveId = signal<string | null>(null);
  readonly isLoadModalOpen = signal(false);
  readonly summaryState = signal<'idle' | 'loading' | 'ready' | 'error'>('idle');
  readonly summaryMessage = signal<string | null>(null);
  readonly loadedGameId = signal<string | null>(null);
  readonly activeGame = this.activeGameService.activeGame;
  readonly dashboardLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.dashboard : appPaths.newGame;
  });
  readonly academyLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.academy : appPaths.newGame;
  });
  readonly matchCenterLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.matchCenter : appPaths.newGame;
  });
  readonly transferMarketLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.transferMarket : appPaths.newGame;
  });
  readonly financesLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.finances : appPaths.newGame;
  });
  readonly leagueTableLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.leagueTable : appPaths.newGame;
  });
  readonly fixturesLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.fixtures : appPaths.newGame;
  });
  readonly squadLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.squad : appPaths.newGame;
  });
  readonly lineupLink = computed(() => {
    const activeGame = this.activeGame();
    return activeGame ? appPaths.lineup : appPaths.newGame;
  });
  readonly featuredPlayerLink = computed(() => {
    const featuredPlayer = this.activeDashboard()?.featuredPlayer;
    return this.activeGame() && featuredPlayer ? buildPlayerPath(featuredPlayer) : appPaths.newGame;
  });
  readonly nextFixtureLabel = computed(() => {
    const fixture = this.activeDashboard()?.nextFixture;

    if (!fixture) {
      return 'No fixture scheduled';
    }

    return `${fixture.homeClub} vs ${fixture.awayClub}`;
  });
  readonly featuredClubs = computed(() => this.clubPreview().slice(0, 4));
  readonly isHeroLoading = computed(() =>
    !!this.activeGame() && !this.activeDashboard() && this.summaryState() !== 'error');
  readonly heroLabel = computed(() => this.activeGame() ? 'Matchday Hub' : 'Career Start');
  readonly heroTitle = computed(() => {
    const dashboard = this.activeDashboard();
    const currentClub = dashboard?.clubName ?? this.activeGame()?.selectedClub;
    const fixture = dashboard?.nextFixture;

    if (!currentClub) {
      return 'Choose your club and begin the climb.';
    }

    if (!fixture) {
      return `${currentClub} are waiting on the next turn of the calendar.`;
    }

    const opponent = fixture.homeClub === currentClub ? fixture.awayClub : fixture.homeClub;
    return `${currentClub} have ${opponent} next.`;
  });
  readonly heroCopy = computed(() => this.activeGame()
    ? (this.activeDashboard()?.momentumNote ?? 'Everything now points toward the next whistle. Settle the XI and play the match that matters.')
    : 'A fresh league is ready. Pick the badge, own the pressure, and start your first season as the manager in charge.');
  readonly featuredPlayerStatus = computed(() => this.activeDashboard()?.featuredPlayer.isStarter
    ? 'Driving the current XI'
    : 'Pushing the starters in training');
  readonly nextOpponent = computed(() => {
    const dashboard = this.activeDashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard || !fixture) {
      return 'Awaiting the fixture board';
    }

    return fixture.homeClub === dashboard.clubName ? fixture.awayClub : fixture.homeClub;
  });
  readonly nextVenue = computed(() => {
    const dashboard = this.activeDashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard || !fixture) {
      return 'Schedule update pending';
    }

    return fixture.homeClub === dashboard.clubName ? 'Home stage' : 'Away trip';
  });
  readonly nextFixtureNarrative = computed(() => {
    const dashboard = this.activeDashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard || !fixture) {
      return 'The board is clear for now, but the camp still feels the pressure of the last turn.';
    }

    return `${dashboard.competitionName} round ${fixture.roundNumber}. ${dashboard.lineup.readiness} is the tone heading into kickoff.`;
  });
  readonly lineupPrompt = computed(() => {
    const lineup = this.activeDashboard()?.lineup;

    if (!lineup) {
      return 'Shape the XI before the next fixture arrives.';
    }

    return `${lineup.formationName} is ${lineup.readiness.toLowerCase()}.`;
  });
  readonly recentResultLabel = computed(() => {
    const result = this.activeDashboard()?.recentResult;

    if (!result) {
      return 'No result on the board yet';
    }

    return `${result.homeClub} ${result.homeGoals}-${result.awayGoals} ${result.awayClub}`;
  });
  readonly recentResultTone = computed(() => {
    const dashboard = this.activeDashboard();
    const result = dashboard?.recentResult;

    if (!dashboard || !result) {
      return 'The next 90 minutes will write the first real story of this save.';
    }

    const goalsFor = result.homeClub === dashboard.clubName ? result.homeGoals : result.awayGoals;
    const goalsAgainst = result.homeClub === dashboard.clubName ? result.awayGoals : result.homeGoals;

    if (goalsFor > goalsAgainst) {
      return 'Momentum is with the squad. The dressing room wants to turn one result into a run.';
    }

    if (goalsFor < goalsAgainst) {
      return 'The last setback is still hanging in the air. The response now matters.';
    }

    return 'The last outing kept things balanced. The next fixture will tip the mood one way or the other.';
  });
  readonly momentumHeading = computed(() => {
    const lineup = this.activeDashboard()?.lineup;

    if (!lineup) {
      return 'Build the edge';
    }

    return lineup.readiness === 'Match sharp'
      ? 'Kickoff energy is rising'
      : lineup.readiness === 'Ready for kickoff'
        ? 'The camp feels steady'
        : 'The pressure is still building';
  });
  readonly nearbyTableRows = computed(() => {
    const table = this.leagueTable();
    const currentClub = this.activeDashboard()?.clubName;
    const currentIndex = table.findIndex((entry) => entry.clubName === currentClub);

    if (currentIndex < 0) {
      return table.slice(0, 4);
    }

    const start = Math.max(0, currentIndex - 1);
    return table.slice(start, Math.min(table.length, currentIndex + 2));
  });
  readonly seasonProgress = computed(() => {
    const fixtures = this.fixtures();
    const played = fixtures.filter((fixture) => fixture.isPlayed).length;
    const remaining = fixtures.length - played;
    const currentRound = fixtures.find((fixture) => fixture.isCurrentRound)?.roundNumber ?? null;
    const totalRounds = fixtures.reduce((maxRound, fixture) => Math.max(maxRound, fixture.roundNumber), 0);

    return {
      played,
      remaining,
      total: fixtures.length,
      currentRound,
      totalRounds
    };
  });
  readonly rivalryEntry = computed(() => {
    const table = this.leagueTable();
    const currentClub = this.activeDashboard()?.clubName;
    const currentIndex = table.findIndex((entry) => entry.clubName === currentClub);

    if (currentIndex < 0) {
      return table[0] ?? null;
    }

    return table[currentIndex - 1] ?? table[currentIndex + 1] ?? null;
  });
  readonly rivalryCopy = computed(() => {
    const dashboard = this.activeDashboard();
    const rival = this.rivalryEntry();

    if (!dashboard || !rival) {
      return 'The table is still waiting for its first real fracture line.';
    }

    if (rival.position < dashboard.leaguePosition) {
      return `${rival.clubName} are the next marker above you on ${rival.points} points. One result can tighten the gap.`;
    }

    if (rival.position > dashboard.leaguePosition) {
      return `${rival.clubName} are hovering below. A slip now would pull them right into the story.`;
    }

    return `${rival.clubName} are level with the pace around you. Goal difference could start to matter.`;
  });
  readonly marketSummary = computed(() => {
    const market = this.transferMarket();

    if (!market) {
      return 'No market movement is live yet.';
    }

    if (market.targets.some((target) => target.isAffordable)) {
      return `${market.targets.filter((target) => target.isAffordable).length} target(s) are inside the current budget.`;
    }

    return 'The board is active, but the best deals may need player sales first.';
  });
  readonly affordableTargetCount = computed(() =>
    this.transferMarket()?.targets.filter((target) => target.isAffordable).length ?? 0);
  readonly marketSpotlightTarget = computed(() =>
    this.transferMarket()?.targets.find((target) => target.isAffordable) ?? null);
  readonly marketSpotlightSale = computed(() =>
    this.marketSpotlightTarget() ? null : (this.transferMarket()?.saleOpportunities[0] ?? null));
  readonly latestTransferActivity = computed(() =>
    this.transferMarket()?.recentActivity[0] ?? null);
  readonly latestFinanceEvent = computed(() =>
    this.financeSummary()?.recentEvents[0] ?? null);
  readonly academySpotlight = computed<AcademyPlayer | null>(() =>
    this.academy()?.spotlightPlayer ?? this.academy()?.players[0] ?? null);
  readonly academySummaryCopy = computed(() => {
    const academy = this.academy();

    if (!academy) {
      return 'The long-term club-building picture is still waiting on the first youth update.';
    }

    return academy.promotionReadyCount > 0
      ? academy.promotionPressure
      : academy.summaryNote;
  });
  readonly recentSaves = computed(() => this.saveLibrary());
  readonly activeSaveSummary = computed(() => {
    const activeGame = this.activeGame();
    return activeGame
      ? (this.saveLibrary().find((save) => save.gameId === activeGame.gameId) ?? null)
      : null;
  });
  readonly spotlightSave = computed(() => this.activeSaveSummary() ?? this.recentSaves()[0] ?? null);
  readonly careerSpotlightCopy = computed(() => {
    const save = this.spotlightSave();
    const activeGame = this.activeGame();
    const dashboard = this.activeDashboard();

    if (!save) {
      return 'Your first club story is still waiting to be written.';
    }

    if (dashboard && activeGame?.gameId === save.gameId) {
      const nextFixture = dashboard.nextFixture;
      if (nextFixture) {
        const opponent = nextFixture.homeClub === dashboard.clubName ? nextFixture.awayClub : nextFixture.homeClub;
        return `${dashboard.clubName} are back in ${dashboard.seasonName}, with ${opponent} next and the table still moving under every decision.`;
      }

      return `${dashboard.clubName} are still in ${dashboard.seasonName}. The save is live and the next chapter is ready when you are.`;
    }

    return `${save.clubName} are waiting in ${save.seasonName}. Pick the journey back up and drive the next chapter from the dugout.`;
  });
  readonly continueLink = computed(() => {
    const activeGame = this.activeGame();
    const save = this.spotlightSave();
    return activeGame && save && activeGame.gameId === save.gameId
      ? appPaths.matchCenter
      : null;
  });

  constructor() {
    effect(() => {
      const activeGame = this.activeGame();

      if (!activeGame) {
        this.activeDashboard.set(null);
        this.academy.set(null);
        this.leagueTable.set([]);
        this.fixtures.set([]);
        this.transferMarket.set(null);
        this.financeSummary.set(null);
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

    effect(() => {
      const pendingAction = this.gameMenuService.pendingAction();

      if (!pendingAction) {
        return;
      }

      if (pendingAction === 'load') {
        this.gameMenuService.clearAction('load');
        this.openLoadFlow();
        return;
      }

      if (!this.activeGame() || this.isSavingGame() || this.loadingSaveId() || this.deletingSaveId()) {
        return;
      }

      this.gameMenuService.clearAction('save');
      this.saveCurrentGame();
    });
  }

  ngOnInit(): void {
    if (this.saveState() === 'idle') {
      this.loadSaveLibrary(this.activeGame()?.gameId ?? undefined);
    }

    if (!this.activeGame() && this.clubPreviewState() === 'idle') {
      this.loadClubPreview();
    }
  }

  ngOnDestroy(): void {
    this.clearDeleteToastTimer();
  }

  saveCurrentGame(): void {
    const activeGame = this.activeGame();

    if (!activeGame || this.isSavingGame() || this.loadingSaveId() || this.deletingSaveId()) {
      return;
    }

    this.isSavingGame.set(true);
    this.saveMessage.set(null);
    this.saveActionMessage.set(null);

    this.api.saveGame(activeGame.gameId).subscribe({
      next: (save) => {
        this.activeGameService.setFromSave(save);
        this.saveActionMessage.set(`${save.saveName} saved.`);
        this.isSavingGame.set(false);
        this.loadSaveLibrary(save.gameId, true);
      },
      error: (error: HttpErrorResponse) => {
        this.saveMessage.set(error.error?.message ?? 'The current career could not be saved right now.');
        this.isSavingGame.set(false);
      }
    });
  }

  openLoadFlow(): void {
    this.isLoadModalOpen.set(true);
    this.loadSaveLibrary(this.activeGame()?.gameId ?? undefined, true);
  }

  closeLoadFlow(): void {
    if (this.loadingSaveId() || this.deletingSaveId()) {
      return;
    }

    this.isLoadModalOpen.set(false);
  }

  loadSave(gameId: string): void {
    if (this.loadingSaveId() || this.isSavingGame() || this.deletingSaveId()) {
      return;
    }

    this.loadingSaveId.set(gameId);
    this.saveMessage.set(null);
    this.saveActionMessage.set(null);

    this.api.getSaveLibrary(gameId).subscribe({
      next: (response) => {
        this.saveLibrary.set(response.saves);
        this.saveState.set('ready');
        this.saveMessage.set(null);

        if (response.selectedSave) {
          this.activeGameService.setFromSave(response.selectedSave);
          this.saveActionMessage.set(`${response.selectedSave.saveName} loaded.`);
        }

        this.loadingSaveId.set(null);
        this.closeLoadFlow();
      },
      error: (error: HttpErrorResponse) => {
        this.saveMessage.set(error.error?.message ?? 'The selected career could not be loaded right now.');
        this.loadingSaveId.set(null);
      }
    });
  }

  deleteSave(save: GameSaveSummary): void {
    if (this.isSavingGame() || this.loadingSaveId() || this.deletingSaveId()) {
      return;
    }

    this.deletingSaveId.set(save.gameId);
    this.dismissDeleteToast();
    this.saveMessage.set(null);
    this.saveActionMessage.set(null);

    this.api.deleteSave(save.gameId).subscribe({
      next: (deletedSave) => {
        if (this.activeGame()?.gameId === deletedSave.gameId) {
          this.activeGameService.clear();
        }

        this.saveActionMessage.set(`${deletedSave.saveName} deleted.`);
        this.deletingSaveId.set(null);
        this.loadSaveLibrary(this.activeGame()?.gameId ?? undefined, true);
      },
      error: (error: HttpErrorResponse) => {
        this.showDeleteToast(error.error?.message ?? 'The selected career could not be deleted right now.');
        this.deletingSaveId.set(null);
      }
    });
  }

  dismissDeleteToast(): void {
    this.clearDeleteToastTimer();
    this.deleteToastMessage.set(null);
  }

  isCurrentSave(gameId: string): boolean {
    return this.activeGame()?.gameId === gameId;
  }

  isLoadingSave(gameId: string): boolean {
    return this.loadingSaveId() === gameId;
  }

  isDeletingSave(gameId: string): boolean {
    return this.deletingSaveId() === gameId;
  }

  private loadActiveSummary(gameId: string): void {
    this.loadedGameId.set(gameId);
    this.summaryState.set('loading');
    this.summaryMessage.set(null);

    forkJoin({
      dashboard: this.api.getClubDashboard(gameId),
      academy: this.api.getAcademy(gameId),
      leagueTable: this.api.getLeagueTable(gameId),
      fixtures: this.api.getFixtures(gameId),
      transferMarket: this.api.getTransferMarket(gameId),
      financeSummary: this.api.getFinance(gameId)
    }).subscribe({
      next: ({ dashboard, academy, leagueTable, fixtures, transferMarket, financeSummary }) => {
        this.activeDashboard.set(dashboard);
        this.academy.set(academy);
        this.leagueTable.set(leagueTable);
        this.fixtures.set(fixtures);
        this.transferMarket.set(transferMarket);
        this.financeSummary.set(financeSummary);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.summaryState.set('ready');
      },
      error: () => {
        this.activeDashboard.set(null);
        this.academy.set(null);
        this.leagueTable.set([]);
        this.fixtures.set([]);
        this.transferMarket.set(null);
        this.financeSummary.set(null);
        this.summaryState.set('error');
        this.summaryMessage.set('This career could not be resumed right now. Start a new campaign to get back to the dugout.');
      }
    });
  }

  private loadSaveLibrary(selectedGameId?: string, preserveActionMessage = false): void {
    this.saveState.set('loading');
    this.saveMessage.set(null);

    if (!preserveActionMessage) {
      this.saveActionMessage.set(null);
    }

    this.api.getSaveLibrary(selectedGameId).subscribe({
      next: (response) => {
        this.saveLibrary.set(response.saves);
        this.saveState.set('ready');
      },
      error: () => {
        this.saveState.set('error');
        this.saveMessage.set('The career archive could not be opened right now.');
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

  private showDeleteToast(message: string): void {
    this.clearDeleteToastTimer();
    this.deleteToastMessage.set(message);

    if (typeof window === 'undefined') {
      return;
    }

    this.deleteToastTimeoutId = window.setTimeout(() => {
      this.deleteToastMessage.set(null);
      this.deleteToastTimeoutId = null;
    }, 4500);
  }

  private clearDeleteToastTimer(): void {
    if (this.deleteToastTimeoutId === null) {
      return;
    }

    clearTimeout(this.deleteToastTimeoutId);
    this.deleteToastTimeoutId = null;
  }
}
