import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { AcademyPlayer, AcademySummary } from '../../models/academy';
import { ClubDashboard } from '../../models/club-dashboard';

@Component({
  selector: 'app-academy',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './academy.component.html',
  styleUrl: './academy.component.scss'
})
export class AcademyComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly dashboard = signal<ClubDashboard | null>(null);
  readonly academy = signal<AcademySummary | null>(null);
  readonly isLoading = signal(true);
  readonly actionPlayerId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly actionMessage = signal<string | null>(null);
  readonly matchCenterLink = computed(() => this.gameId() ? `/match-center/${this.gameId()}` : '/');
  readonly squadLink = computed(() => this.gameId() ? `/squad/${this.gameId()}` : '/');
  readonly topProspect = computed<AcademyPlayer | null>(() =>
    this.academy()?.spotlightPlayer ?? this.academy()?.players[0] ?? null);

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Open a save before stepping into the academy.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadData(gameId);
  }

  promotePlayer(playerId: string): void {
    const gameId = this.gameId();

    if (!gameId || this.actionPlayerId()) {
      return;
    }

    this.actionPlayerId.set(playerId);
    this.errorMessage.set(null);
    this.actionMessage.set(null);

    this.api.promoteAcademyPlayer(gameId, playerId).subscribe({
      next: (result) => {
        this.actionMessage.set(result.summary);
        this.actionPlayerId.set(null);
        this.loadData(gameId, true);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.error?.message ?? 'The promotion could not be completed right now.');
        this.actionPlayerId.set(null);
      }
    });
  }

  isPromoting(playerId: string): boolean {
    return this.actionPlayerId() === playerId;
  }

  private loadData(gameId: string, preserveMessage = false): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    if (!preserveMessage) {
      this.actionMessage.set(null);
    }

    forkJoin({
      dashboard: this.api.getClubDashboard(gameId),
      academy: this.api.getAcademy(gameId)
    }).subscribe({
      next: ({ dashboard, academy }) => {
        this.dashboard.set(dashboard);
        this.academy.set(academy);
        this.activeGameService.syncFromDashboard(gameId, dashboard);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The academy board is unavailable right now. Reopen the save and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
