import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { appPaths, resolveGameId } from '../../core/routing/app-paths';
import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { FixtureSummary } from '../../models/league';

interface FixtureRoundGroup {
  roundNumber: number;
  isCurrentRound: boolean;
  fixtures: FixtureSummary[];
}

@Component({
  selector: 'app-fixtures',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './fixtures.component.html',
  styleUrl: './fixtures.component.scss'
})
export class FixturesComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly fixtures = signal<FixtureSummary[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly matchCenterLink = computed(() => this.gameId() ? appPaths.matchCenter : '/');
  readonly leagueTableLink = computed(() => this.gameId() ? appPaths.leagueTable : '/');
  readonly playedCount = computed(() => this.fixtures().filter((fixture) => fixture.isPlayed).length);
  readonly remainingCount = computed(() => this.fixtures().filter((fixture) => !fixture.isPlayed).length);
  readonly currentRound = computed(() =>
    this.fixtures().find((fixture) => fixture.isCurrentRound)?.roundNumber ?? null);
  readonly roundGroups = computed<FixtureRoundGroup[]>(() => {
    const groups = new Map<number, FixtureSummary[]>();

    for (const fixture of this.fixtures()) {
      const roundFixtures = groups.get(fixture.roundNumber) ?? [];
      roundFixtures.push(fixture);
      groups.set(fixture.roundNumber, roundFixtures);
    }

    return [...groups.entries()]
      .sort((left, right) => left[0] - right[0])
      .map(([roundNumber, fixtures]) => ({
        roundNumber,
        isCurrentRound: fixtures.some((fixture) => fixture.isCurrentRound),
        fixtures
      }));
  });

  ngOnInit(): void {
    const gameId = resolveGameId(this.activeGameService, this.route);

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Open a save before viewing fixtures.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadData(gameId);
  }

  private loadData(gameId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      dashboard: this.api.getClubDashboard(gameId),
      fixtures: this.api.getFixtures(gameId)
    }).subscribe({
      next: ({ dashboard, fixtures }) => {
        this.dashboard.set(dashboard);
        this.fixtures.set(fixtures);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The fixture list is unavailable right now. Reload the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
