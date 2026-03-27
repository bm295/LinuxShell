import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BootstrapSummary } from '../../models/bootstrap-summary';
import { AcademyPromotionResult, AcademySummary } from '../../models/academy';
import { ClubDashboard } from '../../models/club-dashboard';
import { ClubOption } from '../../models/club-option';
import { CreateNewGameResponse } from '../../models/create-new-game-response';
import { FinanceSummary } from '../../models/finance';
import { GameSaveSummary, LoadGameResponse } from '../../models/game-save';
import { HealthStatus } from '../../models/health-status';
import { FixtureSummary, LeagueTableEntry, TopPlayer } from '../../models/league';
import { Lineup, LineupEditor, UpdateLineupRequest } from '../../models/lineup';
import { SimulatedMatchResult } from '../../models/match-simulation';
import { StartNextSeasonResult } from '../../models/season-transition';
import { PlayerDetail, SquadPlayer, UpdatePlayerPositionRequest } from '../../models/squad';
import { TransferActionResult, TransferMarket } from '../../models/transfer-market';

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

  saveGame(gameId: string, saveName?: string | null): Observable<GameSaveSummary> {
    return this.http.post<GameSaveSummary>(`/api/game/save?gameId=${encodeURIComponent(gameId)}`, { saveName: saveName ?? null });
  }

  getSaveLibrary(gameId?: string): Observable<LoadGameResponse> {
    const query = gameId ? `?gameId=${encodeURIComponent(gameId)}` : '';
    return this.http.get<LoadGameResponse>(`/api/game/load${query}`);
  }

  deleteSave(gameId: string): Observable<GameSaveSummary> {
    return this.http.delete<GameSaveSummary>(`/api/game/save?gameId=${encodeURIComponent(gameId)}`);
  }

  getAcademy(gameId: string): Observable<AcademySummary> {
    return this.http.get<AcademySummary>(`/api/academy?gameId=${encodeURIComponent(gameId)}`);
  }

  promoteAcademyPlayer(gameId: string, academyPlayerId: string): Observable<AcademyPromotionResult> {
    return this.http.post<AcademyPromotionResult>(`/api/academy/promote?gameId=${encodeURIComponent(gameId)}`, { academyPlayerId });
  }

  getClubDashboard(gameId: string): Observable<ClubDashboard> {
    return this.http.get<ClubDashboard>(`/api/club/dashboard?gameId=${encodeURIComponent(gameId)}`);
  }

  getLeagueTable(gameId: string): Observable<LeagueTableEntry[]> {
    return this.http.get<LeagueTableEntry[]>(`/api/league/table?gameId=${encodeURIComponent(gameId)}`);
  }

  getFixtures(gameId: string): Observable<FixtureSummary[]> {
    return this.http.get<FixtureSummary[]>(`/api/fixtures?gameId=${encodeURIComponent(gameId)}`);
  }

  getTopPlayers(gameId: string): Observable<TopPlayer[]> {
    return this.http.get<TopPlayer[]>(`/api/league/top-players?gameId=${encodeURIComponent(gameId)}`);
  }

  getSquad(gameId: string): Observable<SquadPlayer[]> {
    return this.http.get<SquadPlayer[]>(`/api/squad?gameId=${encodeURIComponent(gameId)}`);
  }

  getPlayer(gameId: string, playerId: string): Observable<PlayerDetail> {
    return this.http.get<PlayerDetail>(`/api/player/${encodeURIComponent(playerId)}?gameId=${encodeURIComponent(gameId)}`);
  }

  updatePlayerPosition(gameId: string, playerId: string, request: UpdatePlayerPositionRequest): Observable<PlayerDetail> {
    return this.http.put<PlayerDetail>(
      `/api/player/${encodeURIComponent(playerId)}/position?gameId=${encodeURIComponent(gameId)}`,
      request);
  }

  getLineup(gameId: string): Observable<LineupEditor> {
    return this.http.get<LineupEditor>(`/api/lineup?gameId=${encodeURIComponent(gameId)}`);
  }

  saveLineup(gameId: string, request: UpdateLineupRequest): Observable<Lineup> {
    return this.http.post<Lineup>(`/api/lineup?gameId=${encodeURIComponent(gameId)}`, request);
  }

  simulateNextMatch(gameId: string): Observable<SimulatedMatchResult> {
    return this.http.post<SimulatedMatchResult>(`/api/match/simulate-next?gameId=${encodeURIComponent(gameId)}`, {});
  }

  startNextSeason(gameId: string): Observable<StartNextSeasonResult> {
    return this.http.post<StartNextSeasonResult>(`/api/match/start-next-season?gameId=${encodeURIComponent(gameId)}`, {});
  }

  getTransferMarket(gameId: string): Observable<TransferMarket> {
    return this.http.get<TransferMarket>(`/api/transfer/market?gameId=${encodeURIComponent(gameId)}`);
  }

  getFinance(gameId: string): Observable<FinanceSummary> {
    return this.http.get<FinanceSummary>(`/api/finance?gameId=${encodeURIComponent(gameId)}`);
  }

  buyPlayer(gameId: string, playerId: string): Observable<TransferActionResult> {
    return this.http.post<TransferActionResult>(`/api/transfer/buy?gameId=${encodeURIComponent(gameId)}`, { playerId });
  }

  sellPlayer(gameId: string, playerId: string): Observable<TransferActionResult> {
    return this.http.post<TransferActionResult>(`/api/transfer/sell?gameId=${encodeURIComponent(gameId)}`, { playerId });
  }
}
