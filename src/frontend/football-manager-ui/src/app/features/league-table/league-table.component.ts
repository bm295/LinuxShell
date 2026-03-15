import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubDashboard } from '../../models/club-dashboard';
import { LeagueTableEntry } from '../../models/league';

@Component({
  selector: 'app-league-table',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './league-table.component.html',
  styleUrl: './league-table.component.scss'
})
export class LeagueTableComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly table = signal<LeagueTableEntry[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly matchCenterLink = computed(() => this.gameId() ? `/match-center/${this.gameId()}` : '/');
  readonly fixturesLink = computed(() => this.gameId() ? `/fixtures/${this.gameId()}` : '/');
  readonly currentClubRow = computed(() =>
    this.table().find((entry) => entry.clubName === this.dashboard()?.clubName) ?? null);
  readonly pressureRows = computed(() => {
    const table = this.table();
    const currentClub = this.dashboard()?.clubName;
    const currentIndex = table.findIndex((entry) => entry.clubName === currentClub);

    if (currentIndex < 0) {
      return table.slice(0, 3);
    }

    const start = Math.max(0, currentIndex - 1);
    return table.slice(start, Math.min(table.length, currentIndex + 2));
  });

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Open a save before viewing the league table.');
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
      table: this.api.getLeagueTable(gameId)
    }).subscribe({
      next: ({ dashboard, table }) => {
        this.dashboard.set(dashboard);
        this.table.set(table);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The league table is unavailable right now. Reload the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
