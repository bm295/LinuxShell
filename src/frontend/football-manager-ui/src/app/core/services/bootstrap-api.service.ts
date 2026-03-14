import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BootstrapSummary } from '../../models/bootstrap-summary';
import { HealthStatus } from '../../models/health-status';

@Injectable({ providedIn: 'root' })
export class BootstrapApiService {
  private readonly http = inject(HttpClient);

  getHealth(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>('/api/health');
  }

  getBootstrapSummary(): Observable<BootstrapSummary> {
    return this.http.get<BootstrapSummary>('/api/bootstrap/summary');
  }
}
