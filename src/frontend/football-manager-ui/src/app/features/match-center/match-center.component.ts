import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { SimulatedMatchResult } from '../../models/match-simulation';

@Component({
  selector: 'app-match-center',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './match-center.component.html',
  styleUrl: './match-center.component.scss'
})
export class MatchCenterComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly simulationResult = signal<SimulatedMatchResult | null>(null);
  readonly isLoading = signal(true);
  readonly isSimulating = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly actionMessage = signal<string | null>(null);
  readonly dashboardLink = computed(() => this.gameId() ? `/dashboard/${this.gameId()}` : '/');
  readonly lineupLink = computed(() => this.gameId() ? `/lineup/${this.gameId()}` : '/');
  readonly homeLink = computed(() => '/');
  readonly nextFixture = computed(() => this.dashboard()?.nextFixture ?? null);
  readonly opponentName = computed(() => {
    const dashboard = this.dashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard || !fixture) {
      return 'Awaiting the next fixture';
    }

    return fixture.homeClub === dashboard.clubName ? fixture.awayClub : fixture.homeClub;
  });
  readonly venueLabel = computed(() => {
    const dashboard = this.dashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard || !fixture) {
      return 'Schedule complete';
    }

    return fixture.homeClub === dashboard.clubName ? 'Home night' : 'Away day';
  });
  readonly injuryCount = computed(() =>
    this.dashboard()?.squadSummary.players.filter((player) => player.isInjured).length ?? 0);
  readonly heroTitle = computed(() => {
    const dashboard = this.dashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard) {
      return 'Match Center';
    }

    if (!fixture) {
      return `${dashboard.clubName} have reached the end of the current schedule.`;
    }

    const opponent = fixture.homeClub === dashboard.clubName ? fixture.awayClub : fixture.homeClub;
    return `${dashboard.clubName} vs ${opponent}`;
  });
  readonly heroCopy = computed(() => {
    const dashboard = this.dashboard();
    const fixture = dashboard?.nextFixture;

    if (!dashboard) {
      return 'Load a save to bring the next fixture into focus.';
    }

    if (!fixture) {
      return 'There is no unplayed fixture left in this save. Review the latest result and season standing instead.';
    }

    return `Round ${fixture.roundNumber} in ${dashboard.competitionName}. ${dashboard.momentumNote}`;
  });

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Start a new game before opening Match Center.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadDashboard(gameId);
  }

  simulateMatch(): void {
    const gameId = this.gameId();

    if (!gameId || !this.dashboard()?.nextFixture || this.isSimulating()) {
      return;
    }

    this.isSimulating.set(true);
    this.errorMessage.set(null);
    this.actionMessage.set(null);

    this.api.simulateNextMatch(gameId).subscribe({
      next: (result) => {
        this.simulationResult.set(result);
        this.actionMessage.set(result.summary);
        this.isSimulating.set(false);
        this.loadDashboard(gameId, true);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'The match could not be simulated right now.');
        this.isSimulating.set(false);
      }
    });
  }

  private loadDashboard(gameId: string, preserveResult = false): void {
    if (!preserveResult) {
      this.simulationResult.set(null);
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getClubDashboard(gameId).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The match briefing is unavailable. Reopen the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
