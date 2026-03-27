import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { appPaths, resolveGameId } from '../../core/routing/app-paths';
import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { TopPlayer } from '../../models/league';

interface RankedTopPlayer extends TopPlayer {
  rank: number;
}

@Component({
  selector: 'app-top-players',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './top-players.component.html',
  styleUrl: './top-players.component.scss'
})
export class TopPlayersComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly topPlayers = signal<TopPlayer[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly leagueTableLink = computed(() => this.gameId() ? appPaths.leagueTable : '/');
  readonly fixturesLink = computed(() => this.gameId() ? appPaths.fixtures : '/');
  readonly matchCenterLink = computed(() => this.gameId() ? appPaths.matchCenter : '/');
  readonly leaderboard = computed<RankedTopPlayer[]>(() =>
    this.topPlayers().map((player, index) => ({
      ...player,
      rank: index + 1
    })));
  readonly leader = computed(() => this.topPlayers()[0] ?? null);
  readonly managedClubEntries = computed(() => {
    const clubName = this.dashboard()?.clubName;

    if (!clubName) {
      return [];
    }

    return this.topPlayers().filter((player) => player.clubName === clubName);
  });
  readonly totalAwardsOnBoard = computed(() =>
    this.topPlayers().reduce((total, player) => total + player.mvpAwards, 0));

  ngOnInit(): void {
    const gameId = resolveGameId(this.activeGameService, this.route);

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Open a save before viewing top players.');
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
      topPlayers: this.api.getTopPlayers(gameId)
    }).subscribe({
      next: ({ dashboard, topPlayers }) => {
        this.dashboard.set(dashboard);
        this.topPlayers.set(topPlayers);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The top players board is unavailable right now. Reload the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
