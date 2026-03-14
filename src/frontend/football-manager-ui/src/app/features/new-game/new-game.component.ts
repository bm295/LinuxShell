import { Component, inject, OnInit, signal } from '@angular/core';

import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { BootstrapSummary } from '../../models/bootstrap-summary';

@Component({
  selector: 'app-new-game',
  standalone: true,
  templateUrl: './new-game.component.html',
  styleUrl: './new-game.component.scss'
})
export class NewGameComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);

  readonly summary = signal<BootstrapSummary | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getBootstrapSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Bootstrap summary is unavailable until the backend and database finish starting.');
        this.isLoading.set(false);
      }
    });
  }
}
