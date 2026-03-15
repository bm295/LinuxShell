export interface GameSaveSummary {
  gameId: string;
  saveName: string;
  clubName: string;
  seasonName: string;
  createdAt: string;
  lastSavedAt: string;
}

export interface LoadGameResponse {
  selectedSave: GameSaveSummary | null;
  saves: GameSaveSummary[];
}
