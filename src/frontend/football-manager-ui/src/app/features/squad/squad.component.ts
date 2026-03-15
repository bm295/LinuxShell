import { TitleCasePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { SquadPlayer } from '../../models/squad';

@Component({
  selector: 'app-squad',
  standalone: true,
  imports: [RouterLink, TitleCasePipe],
  templateUrl: './squad.component.html',
  styleUrl: './squad.component.scss'
})
export class SquadComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly players = signal<SquadPlayer[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly starterCount = computed(() => this.players().filter((player) => player.isStarter).length);
  readonly unavailableCount = computed(() => this.players().filter((player) => player.isInjured).length);
  readonly averageRating = computed(() => this.calculateAverage(this.players().map((player) => player.overallRating)));
  readonly dashboardLink = computed(() => {
    const gameId = this.gameId();
    return gameId ? `/dashboard/${gameId}` : '/';
  });
  readonly lineupLink = computed(() => {
    const gameId = this.gameId();
    return gameId ? `/lineup/${gameId}` : '/';
  });

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Start a new game first.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadSquad(gameId);
  }

  private loadSquad(gameId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      players: this.api.getSquad(gameId),
      dashboard: this.api.getClubDashboard(gameId)
    }).subscribe({
      next: ({ players, dashboard }) => {
        this.players.set(players);
        this.dashboard.set(dashboard);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The squad room is closed right now. Reload the save or start a new career.');
        this.isLoading.set(false);
      }
    });
  }

  private calculateAverage(values: number[]): number {
    return values.length === 0
      ? 0
      : Math.round(values.reduce((total, value) => total + value, 0) / values.length);
  }
}
