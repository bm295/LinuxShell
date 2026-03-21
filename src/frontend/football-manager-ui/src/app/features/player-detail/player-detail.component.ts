import { TitleCasePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { appPaths, parsePlayerRouteKey, resolveGameId } from '../../core/routing/app-paths';
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

  readonly availablePositions = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];
  readonly gameId = signal<string | null>(null);
  readonly player = signal<PlayerDetail | null>(null);
  readonly selectedPosition = signal<string>('');
  readonly isLoading = signal(true);
  readonly isUpdatingPosition = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly positionFeedbackMessage = signal<string | null>(null);
  readonly positionFeedbackTone = signal<'idle' | 'success' | 'error'>('idle');
  readonly clubName = computed(() => this.activeGameService.activeGame()?.selectedClub ?? 'Squad');
  readonly squadLink = computed(() => this.gameId() ? appPaths.squad : '/');
  readonly lineupLink = computed(() => this.gameId() ? appPaths.lineup : '/');
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
  readonly canUpdatePosition = computed(() => {
    const player = this.player();

    return !!player &&
      !!this.gameId() &&
      !this.isUpdatingPosition() &&
      !!this.selectedPosition() &&
      this.selectedPosition() !== player.position;
  });

  ngOnInit(): void {
    const gameId = resolveGameId(this.activeGameService, this.route);
    const playerId = this.route.snapshot.paramMap.get('playerId');
    const playerKey = this.route.snapshot.paramMap.get('playerKey');

    if (!gameId || (!playerId && !playerKey)) {
      this.errorMessage.set('Missing player route information. Open the squad again and choose a player.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    if (playerId) {
      this.loadPlayer(gameId, playerId);
      return;
    }

    this.loadPlayerByKey(gameId, playerKey!);
  }

  onPositionChange(event: Event): void {
    this.selectedPosition.set((event.target as HTMLSelectElement).value);
    this.positionFeedbackMessage.set(null);
    this.positionFeedbackTone.set('idle');
  }

  canDeactivate(): boolean {
    return !this.isUpdatingPosition();
  }

  preventNavigationWhileUpdating(event: Event): void {
    if (!this.isUpdatingPosition()) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    this.positionFeedbackMessage.set('Wait for the role change to finish before leaving this page.');
    this.positionFeedbackTone.set('error');
  }

  updatePosition(): void {
    const gameId = this.gameId();
    const player = this.player();
    const position = this.selectedPosition();

    if (!gameId || !player || !position || position === player.position || this.isUpdatingPosition()) {
      return;
    }

    this.isUpdatingPosition.set(true);
    this.positionFeedbackMessage.set(null);
    this.positionFeedbackTone.set('idle');

    this.api.updatePlayerPosition(gameId, player.id, { position }).subscribe({
      next: (updatedPlayer) => {
        this.player.set(updatedPlayer);
        this.selectedPosition.set(updatedPlayer.position);
        this.positionFeedbackMessage.set(`${updatedPlayer.name} now trains as a ${updatedPlayer.position.toLowerCase()}. Review the line up if needed.`);
        this.positionFeedbackTone.set('success');
        this.isUpdatingPosition.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.positionFeedbackMessage.set(error.error?.message ?? 'The staff could not confirm that position change right now.');
        this.positionFeedbackTone.set('error');
        this.isUpdatingPosition.set(false);
      }
    });
  }

  private loadPlayer(gameId: string, playerId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getPlayer(gameId, playerId).subscribe({
      next: (player) => {
        this.player.set(player);
        this.selectedPosition.set(player.position);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('That player file is unavailable. Head back to the squad board and try again.');
        this.isLoading.set(false);
      }
    });
  }

  private loadPlayerByKey(gameId: string, playerKey: string): void {
    const squadNumber = parsePlayerRouteKey(playerKey);
    if (squadNumber === null) {
      this.errorMessage.set('That player link is invalid. Head back to the squad board and choose the profile again.');
      this.isLoading.set(false);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getSquad(gameId).subscribe({
      next: (players) => {
        const player = players.find((candidate) => candidate.squadNumber === squadNumber);

        if (!player) {
          this.errorMessage.set('That player is no longer in the active squad. Head back to the squad board and pick another profile.');
          this.isLoading.set(false);
          return;
        }

        this.loadPlayer(gameId, player.id);
      },
      error: () => {
        this.errorMessage.set('The squad board is unavailable right now. Head back home and reload the current save.');
        this.isLoading.set(false);
      }
    });
  }
}
