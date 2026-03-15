import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BootstrapSummary } from '../../models/bootstrap-summary';
import { ClubDashboard } from '../../models/club-dashboard';
import { ClubOption } from '../../models/club-option';
import { CreateNewGameResponse } from '../../models/create-new-game-response';
import { HealthStatus } from '../../models/health-status';
import { Lineup, LineupEditor, UpdateLineupRequest } from '../../models/lineup';
import { PlayerDetail, SquadPlayer } from '../../models/squad';

@Injectable({ providedIn: 'root' })
export class BootstrapApiService {
  private readonly http = inject(HttpClient);

  getHealth(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>('/api/health');
  }

  getBootstrapSummary(): Observable<BootstrapSummary> {
    return this.http.get<BootstrapSummary>('/api/bootstrap/summary');
  }

  getAvailableClubs(): Observable<ClubOption[]> {
    return this.http.get<ClubOption[]>('/api/game/clubs');
  }

  createNewGame(clubId: string): Observable<CreateNewGameResponse> {
    return this.http.post<CreateNewGameResponse>('/api/game/new', { clubId });
  }

  getClubDashboard(gameId: string): Observable<ClubDashboard> {
    return this.http.get<ClubDashboard>(`/api/club/dashboard?gameId=${encodeURIComponent(gameId)}`);
  }

  getSquad(gameId: string): Observable<SquadPlayer[]> {
    return this.http.get<SquadPlayer[]>(`/api/squad?gameId=${encodeURIComponent(gameId)}`);
  }

  getPlayer(gameId: string, playerId: string): Observable<PlayerDetail> {
    return this.http.get<PlayerDetail>(`/api/player/${encodeURIComponent(playerId)}?gameId=${encodeURIComponent(gameId)}`);
  }

  getLineup(gameId: string): Observable<LineupEditor> {
    return this.http.get<LineupEditor>(`/api/lineup?gameId=${encodeURIComponent(gameId)}`);
  }

  saveLineup(gameId: string, request: UpdateLineupRequest): Observable<Lineup> {
    return this.http.post<Lineup>(`/api/lineup?gameId=${encodeURIComponent(gameId)}`, request);
  }
}
