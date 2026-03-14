import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';

import { BootstrapApiService } from '../../core/services/bootstrap-api.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);

  readonly requestState = signal<'loading' | 'online' | 'offline'>('loading');
  readonly statusMessage = signal('Checking backend health...');
  readonly lastChecked = signal<Date | null>(null);
  readonly statusLabel = computed(() => {
    switch (this.requestState()) {
      case 'online':
        return 'Backend online';
      case 'offline':
        return 'Backend unavailable';
      default:
        return 'Checking backend';
    }
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.requestState.set('loading');
    this.statusMessage.set('Checking backend health...');

    this.api.getHealth().subscribe({
      next: (response) => {
        const isHealthy = response.status.toLowerCase() === 'ok';
        this.requestState.set(isHealthy ? 'online' : 'offline');
        this.statusMessage.set(isHealthy ? 'Health endpoint returned status: ok.' : 'Backend responded, but status was not ok.');
        this.lastChecked.set(new Date());
      },
      error: () => {
        this.requestState.set('offline');
        this.statusMessage.set('The Angular app could not reach the backend. Start the API and PostgreSQL stack first.');
        this.lastChecked.set(new Date());
      }
    });
  }
}
