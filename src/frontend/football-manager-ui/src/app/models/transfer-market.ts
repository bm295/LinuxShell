export interface TransferMarketPlayer {
  playerId: string;
  name: string;
  position: string;
  squadNumber: number;
  clubName: string;
  overallRating: number;
  fitness: number;
  morale: number;
  fee: number;
  isAffordable: boolean;
}

export interface SaleOpportunity {
  playerId: string;
  name: string;
  position: string;
  squadNumber: number;
  overallRating: number;
  fitness: number;
  morale: number;
  fee: number;
  suggestedBuyer: string;
}

export interface TransferActivity {
  transferId: string;
  playerName: string;
  fromClub: string;
  toClub: string;
  fee: number;
  completedAt: string;
  isIncoming: boolean;
}

export interface TransferMarket {
  availableBudget: number;
  targets: TransferMarketPlayer[];
  saleOpportunities: SaleOpportunity[];
  recentActivity: TransferActivity[];
}

export interface TransferActionResult {
  availableBudget: number;
  transfer: TransferActivity;
  summary: string;
}
