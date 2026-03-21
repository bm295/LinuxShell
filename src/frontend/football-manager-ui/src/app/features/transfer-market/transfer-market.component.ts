import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { appPaths, resolveGameId } from '../../core/routing/app-paths';
import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { TransferMarket, TransferMarketPlayer } from '../../models/transfer-market';

@Component({
  selector: 'app-transfer-market',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './transfer-market.component.html',
  styleUrl: './transfer-market.component.scss'
})
export class TransferMarketComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly market = signal<TransferMarket | null>(null);
  readonly isLoading = signal(true);
  readonly actionPlayerId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly actionMessage = signal<string | null>(null);
  readonly matchCenterLink = computed(() => this.gameId() ? appPaths.matchCenter : '/');
  readonly squadLink = computed(() => this.gameId() ? appPaths.squad : '/');
  readonly featuredTarget = computed<TransferMarketPlayer | null>(() => {
    const market = this.market();

    return market?.targets.find((target) => target.isAffordable)
      ?? market?.targets[0]
      ?? null;
  });
  readonly affordableTargetCount = computed(() =>
    this.market()?.targets.filter((target) => target.isAffordable).length ?? 0);

  ngOnInit(): void {
    const gameId = resolveGameId(this.activeGameService, this.route);

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Open a save before entering the market.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadData(gameId);
  }

  buyPlayer(playerId: string): void {
    const gameId = this.gameId();

    if (!gameId || this.actionPlayerId()) {
      return;
    }

    this.actionPlayerId.set(playerId);
    this.errorMessage.set(null);
    this.actionMessage.set(null);

    this.api.buyPlayer(gameId, playerId).subscribe({
      next: (result) => {
        this.actionMessage.set(result.summary);
        this.actionPlayerId.set(null);
        this.loadData(gameId, true);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'The deal could not be completed right now.');
        this.actionPlayerId.set(null);
      }
    });
  }

  sellPlayer(playerId: string): void {
    const gameId = this.gameId();

    if (!gameId || this.actionPlayerId()) {
      return;
    }

    this.actionPlayerId.set(playerId);
    this.errorMessage.set(null);
    this.actionMessage.set(null);

    this.api.sellPlayer(gameId, playerId).subscribe({
      next: (result) => {
        this.actionMessage.set(result.summary);
        this.actionPlayerId.set(null);
        this.loadData(gameId, true);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'The sale could not be completed right now.');
        this.actionPlayerId.set(null);
      }
    });
  }

  private loadData(gameId: string, preserveMessage = false): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    if (!preserveMessage) {
      this.actionMessage.set(null);
    }

    forkJoin({
      dashboard: this.api.getClubDashboard(gameId),
      market: this.api.getTransferMarket(gameId)
    }).subscribe({
      next: ({ dashboard, market }) => {
        this.dashboard.set(dashboard);
        this.market.set(market);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The market board is unavailable. Reopen the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
