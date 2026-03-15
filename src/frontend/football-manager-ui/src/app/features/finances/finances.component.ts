import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { FinanceSummary } from '../../models/finance';

@Component({
  selector: 'app-finances',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './finances.component.html',
  styleUrl: './finances.component.scss'
})
export class FinancesComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly finance = signal<FinanceSummary | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly matchCenterLink = computed(() => this.gameId() ? `/match-center/${this.gameId()}` : '/');
  readonly transferMarketLink = computed(() => this.gameId() ? `/transfer-market/${this.gameId()}` : '/');
  readonly latestEvent = computed(() => this.finance()?.recentEvents[0] ?? null);

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Open a save before stepping into the boardroom.');
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
      finance: this.api.getFinance(gameId)
    }).subscribe({
      next: ({ dashboard, finance }) => {
        this.dashboard.set(dashboard);
        this.finance.set(finance);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The finance office is not ready right now. Reopen the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
