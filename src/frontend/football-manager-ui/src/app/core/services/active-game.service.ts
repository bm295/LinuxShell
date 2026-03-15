import { Injectable, signal } from '@angular/core';

import { ClubDashboard } from '../../models/club-dashboard';
import { CreateNewGameResponse } from '../../models/create-new-game-response';
import { GameSaveSummary } from '../../models/game-save';

const storageKey = 'football-manager.active-game';

export interface ActiveGameState {
  gameId: string;
  selectedClub: string;
  seasonId: string | null;
  seasonName: string | null;
  saveName: string | null;
}

@Injectable({ providedIn: 'root' })
export class ActiveGameService {
  readonly activeGame = signal<ActiveGameState | null>(this.readState());

  setFromCreate(response: CreateNewGameResponse): void {
    this.persistState({
      gameId: response.gameId,
      selectedClub: response.selectedClub,
      seasonId: response.seasonId,
      seasonName: response.seasonName,
      saveName: null
    });
  }

  setFromSave(summary: GameSaveSummary): void {
    this.persistState({
      gameId: summary.gameId,
      selectedClub: summary.clubName,
      seasonId: null,
      seasonName: summary.seasonName,
      saveName: summary.saveName
    });
  }

  syncFromDashboard(gameId: string, dashboard: ClubDashboard): void {
    const current = this.activeGame();

    this.persistState({
      gameId,
      selectedClub: dashboard.clubName,
      seasonId: current?.gameId === gameId ? current.seasonId : null,
      seasonName: dashboard.seasonName,
      saveName: current?.gameId === gameId ? current.saveName : null
    });
  }

  clear(): void {
    this.persistState(null);
  }

  private persistState(state: ActiveGameState | null): void {
    if (this.isSameState(this.activeGame(), state)) {
      return;
    }

    this.activeGame.set(state);

    if (typeof localStorage === 'undefined') {
      return;
    }

    if (state === null) {
      localStorage.removeItem(storageKey);
      return;
    }

    localStorage.setItem(storageKey, JSON.stringify(state));
  }

  private readState(): ActiveGameState | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    const rawValue = localStorage.getItem(storageKey);
    if (!rawValue) {
      return null;
    }

    try {
      const parsed = JSON.parse(rawValue) as Partial<ActiveGameState>;

      if (!parsed.gameId || !parsed.selectedClub) {
        return null;
      }

      return {
        gameId: parsed.gameId,
        selectedClub: parsed.selectedClub,
        seasonId: parsed.seasonId ?? null,
        seasonName: parsed.seasonName ?? null,
        saveName: parsed.saveName ?? null
      };
    } catch {
      localStorage.removeItem(storageKey);
      return null;
    }
  }

  private isSameState(left: ActiveGameState | null, right: ActiveGameState | null): boolean {
    return left?.gameId === right?.gameId &&
      left?.selectedClub === right?.selectedClub &&
      left?.seasonId === right?.seasonId &&
      left?.seasonName === right?.seasonName &&
      left?.saveName === right?.saveName;
  }
}
