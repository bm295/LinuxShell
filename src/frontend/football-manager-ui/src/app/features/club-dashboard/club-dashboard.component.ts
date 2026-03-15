import { CurrencyPipe, DatePipe, TitleCasePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';

@Component({
  selector: 'app-club-dashboard',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, TitleCasePipe],
  templateUrl: './club-dashboard.component.html',
  styleUrl: './club-dashboard.component.scss'
})
export class ClubDashboardComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly currentGameId = signal<string | null>(null);
  readonly matchCenterLink = computed(() => this.currentGameId() ? `/match-center/${this.currentGameId()}` : '/');
  readonly squadLink = computed(() => this.currentGameId() ? `/squad/${this.currentGameId()}` : '/');
  readonly lineupLink = computed(() => this.currentGameId() ? `/lineup/${this.currentGameId()}` : '/');

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Start a new game first.');
      this.isLoading.set(false);
      return;
    }

    this.currentGameId.set(gameId);
    this.loadDashboard(gameId);
  }

  loadDashboard(gameId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getClubDashboard(gameId).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Dashboard data is unavailable. Start a new game again if the saved game was not created.');
        this.isLoading.set(false);
      }
    });
  }
}
