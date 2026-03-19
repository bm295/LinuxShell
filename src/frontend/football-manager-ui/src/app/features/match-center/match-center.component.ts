import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { AcademyDevelopmentChange, PlayerDevelopmentChange, SimulatedMatchResult } from '../../models/match-simulation';

type MatchReportTab = 'review' | 'players' | 'academy';

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
  readonly isReportModalOpen = signal(false);
  readonly activeReportTab = signal<MatchReportTab>('review');
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
  readonly seniorDevelopment = computed(() => {
    const result = this.simulationResult();

    if (!result) {
      return [];
    }

    return [...result.seniorPlayerDevelopment].sort((left, right) =>
      Number(right.playedMatch) - Number(left.playedMatch) ||
      left.squadNumber - right.squadNumber ||
      left.name.localeCompare(right.name));
  });
  readonly academyDevelopment = computed(() => {
    const result = this.simulationResult();

    if (!result) {
      return [];
    }

    return [...result.academyDevelopment].sort((left, right) =>
      right.developmentProgressDelta - left.developmentProgressDelta ||
      right.overallDelta - left.overallDelta ||
      left.name.localeCompare(right.name));
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
        this.activeReportTab.set('review');
        this.isReportModalOpen.set(true);
        this.isSimulating.set(false);
        this.loadDashboard(gameId, true);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'The match could not be simulated right now.');
        this.isSimulating.set(false);
      }
    });
  }

  openReport(tab: MatchReportTab = 'review'): void {
    if (!this.simulationResult()) {
      return;
    }

    this.activeReportTab.set(tab);
    this.isReportModalOpen.set(true);
  }

  closeReport(): void {
    this.isReportModalOpen.set(false);
  }

  selectReportTab(tab: MatchReportTab): void {
    this.activeReportTab.set(tab);
  }

  formatDelta(delta: number): string {
    return delta > 0 ? `+${delta}` : `${delta}`;
  }

  hasSeniorCoreShift(change: PlayerDevelopmentChange): boolean {
    return change.attackDelta !== 0 || change.defenseDelta !== 0 || change.passingDelta !== 0 || change.overallDelta !== 0;
  }

  hasAcademyCoreShift(change: AcademyDevelopmentChange): boolean {
    return change.attackDelta !== 0 || change.defenseDelta !== 0 || change.passingDelta !== 0 || change.overallDelta !== 0;
  }

  seniorDevelopmentNote(change: PlayerDevelopmentChange): string {
    if (this.hasSeniorCoreShift(change)) {
      return change.playedMatch
        ? 'Core attributes moved after match exposure.'
        : 'Squad rotation still pushed development and condition changes.';
    }

    return change.playedMatch
      ? 'No core attribute swing this round, but condition and morale still moved.'
      : 'No core attribute swing this round. Recovery work carried the update.';
  }

  academyDevelopmentNote(change: AcademyDevelopmentChange): string {
    if (this.hasAcademyCoreShift(change)) {
      return 'Training delivered a visible technical gain this round.';
    }

    if (change.developmentProgressDelta > 0) {
      return 'Progress moved forward even without a visible core attribute jump.';
    }

    return 'This round held the line more than it lifted the profile.';
  }

  private loadDashboard(gameId: string, preserveResult = false): void {
    if (!preserveResult) {
      this.simulationResult.set(null);
      this.isReportModalOpen.set(false);
      this.activeReportTab.set('review');
      this.isLoading.set(true);
    }

    this.errorMessage.set(null);

    this.api.getClubDashboard(gameId).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.errorMessage.set(null);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set(
          preserveResult
            ? 'The latest match briefing could not fully refresh, but the post-match report is still available.'
            : 'The match briefing is unavailable. Reopen the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
