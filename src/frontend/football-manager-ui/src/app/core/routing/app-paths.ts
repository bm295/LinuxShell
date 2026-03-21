import { ActivatedRoute } from '@angular/router';

import { ActiveGameService } from '../services/active-game.service';

export const appPaths = {
  home: '/',
  newGame: '/new-game',
  dashboard: '/dashboard',
  academy: '/academy',
  squad: '/squad',
  lineup: '/lineup',
  matchCenter: '/match-center',
  leagueTable: '/league-table',
  fixtures: '/fixtures',
  transferMarket: '/transfer-market',
  finances: '/finances'
} as const;

export interface PlayerRouteTarget {
  squadNumber: number;
  name: string;
}

export function resolveGameId(activeGameService: ActiveGameService, route: ActivatedRoute): string | null {
  return route.snapshot.paramMap.get('gameId') ?? activeGameService.activeGame()?.gameId ?? null;
}

export function buildPlayerPath(player: PlayerRouteTarget): string {
  return `/player/${player.squadNumber}-${slugify(player.name)}`;
}

export function parsePlayerRouteKey(playerKey: string | null): number | null {
  if (!playerKey) {
    return null;
  }

  const match = /^(\d+)/.exec(playerKey.trim());
  if (!match) {
    return null;
  }

  const squadNumber = Number.parseInt(match[1], 10);
  return Number.isNaN(squadNumber) ? null : squadNumber;
}

function slugify(value: string): string {
  const normalizedValue = value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

  return normalizedValue || 'player';
}
