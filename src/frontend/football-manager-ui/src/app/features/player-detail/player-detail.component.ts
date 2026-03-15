import { TitleCasePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { PlayerDetail } from '../../models/squad';

@Component({
  selector: 'app-player-detail',
  standalone: true,
  imports: [RouterLink, TitleCasePipe],
  templateUrl: './player-detail.component.html',
  styleUrl: './player-detail.component.scss'
})
export class PlayerDetailComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly gameId = signal<string | null>(null);
  readonly player = signal<PlayerDetail | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly clubName = computed(() => this.activeGameService.activeGame()?.selectedClub ?? 'Squad');
  readonly squadLink = computed(() => {
    const gameId = this.gameId();
    return gameId ? `/squad/${gameId}` : '/';
  });
  readonly lineupLink = computed(() => {
    const gameId = this.gameId();
    return gameId ? `/lineup/${gameId}` : '/';
  });
  readonly metricCards = computed(() => {
    const player = this.player();
    if (!player) {
      return [];
    }

    return [
      { label: 'Attack', value: player.attack },
      { label: 'Defense', value: player.defense },
      { label: 'Passing', value: player.passing },
      { label: 'Fitness', value: player.fitness },
      { label: 'Morale', value: player.morale }
    ];
  });

  ngOnInit(): void {
    const gameId = this.route.snapshot.paramMap.get('gameId');
    const playerId = this.route.snapshot.paramMap.get('playerId');

    if (!gameId || !playerId) {
      this.errorMessage.set('Missing player route information. Open the squad again and choose a player.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadPlayer(gameId, playerId);
  }

  private loadPlayer(gameId: string, playerId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getPlayer(gameId, playerId).subscribe({
      next: (player) => {
        this.player.set(player);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('That player file is unavailable. Head back to the squad board and try again.');
        this.isLoading.set(false);
      }
    });
  }
}
