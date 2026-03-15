export interface FinanceEvent {
  id: string;
  type: string;
  description: string;
  amount: number;
  occurredAt: string;
  isIncome: boolean;
}

export interface FinanceSummary {
  clubName: string;
  currentBudget: number;
  wageTotal: number;
  transferSpending: number;
  transferIncome: number;
  matchIncome: number;
  recentIncome: number;
  recentIncomeLabel: string;
  totalIncome: number;
  totalExpenses: number;
  trendSummary: string;
  boardConfidence: string;
  boardConfidenceNote: string;
  recentEvents: FinanceEvent[];
}
