import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { ClubOption } from '../../models/club-option';

@Component({
  selector: 'app-new-game',
  standalone: true,
  imports: [CurrencyPipe],
  templateUrl: './new-game.component.html',
  styleUrl: './new-game.component.scss'
})
export class NewGameComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly router = inject(Router);

  readonly clubs = signal<ClubOption[]>([]);
  readonly selectedClubId = signal<string | null>(null);
  readonly selectedClub = computed(() => this.clubs().find((club) => club.id === this.selectedClubId()) ?? null);
  readonly isLoading = signal(true);
  readonly isStarting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadClubs();
  }

  loadClubs(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getAvailableClubs().subscribe({
      next: (clubs) => {
        this.clubs.set(clubs);
        this.selectedClubId.set(clubs[0]?.id ?? null);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Club selection is unavailable until the backend and database finish starting.');
        this.isLoading.set(false);
      }
    });
  }

  selectClub(clubId: string): void {
    this.selectedClubId.set(clubId);
  }

  startGame(): void {
    const clubId = this.selectedClubId();
    if (!clubId) {
      this.errorMessage.set('Select a club before starting a game.');
      return;
    }

    this.isStarting.set(true);
    this.errorMessage.set(null);

    this.api.createNewGame(clubId).subscribe({
      next: (response) => {
        this.activeGameService.setFromCreate(response);
        this.router.navigate(['/dashboard', response.gameId]);
      },
      error: () => {
        this.errorMessage.set('The game could not be created. Verify the backend is running and try again.');
        this.isStarting.set(false);
      }
    });
  }
}
