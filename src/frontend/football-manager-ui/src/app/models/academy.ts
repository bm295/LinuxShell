export interface AcademyPlayer {
  playerId: string;
  name: string;
  position: string;
  age: number;
  overallRating: number;
  potential: number;
  developmentProgress: number;
  promotionReadiness: number;
  trainingFocus: string;
  trainingStatus: string;
  isPromotionReady: boolean;
  promotionNote: string;
}

export interface AcademySummary {
  clubName: string;
  academyDepth: number;
  promotionReadyCount: number;
  averagePotential: number;
  averageReadiness: number;
  summaryNote: string;
  promotionPressure: string;
  spotlightPlayer: AcademyPlayer | null;
  players: AcademyPlayer[];
}

export interface AcademyPromotionResult {
  academyPlayerId: string;
  seniorPlayerId: string;
  playerName: string;
  squadNumber: number;
  academyDepth: number;
  seniorSquadCount: number;
  summary: string;
}
